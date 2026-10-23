"""Compose original wizard-academy chamber music and magical SFX.
Requires numpy. Deterministic synthesis; no samples, downloads or copyrighted tunes.
Circular mixing/reverb preserves music tails across loop boundaries.
"""
from pathlib import Path
import json, wave, hashlib
import numpy as np
ROOT=Path(__file__).resolve().parents[1]
RATE=22050
rng=np.random.default_rng(731)
TAU=2*np.pi

def frequency(midi): return 440*2**((midi-69)/12)
def voice(midi,duration,kind):
    t=np.arange(round(duration*RATE))/RATE; f=frequency(midi)
    attack=np.minimum(t/({'strings':.25,'choir':.4,'flute':.07}.get(kind,.008)),1)
    release=np.minimum((duration-t)/(.35 if kind in ('strings','choir') else .08),1)
    if kind=='harp':
        signal=sum(np.sin(TAU*f*h*t)*np.exp(-t*(1.8+h*.65))/h**1.5 for h in range(1,6))
    elif kind=='celesta':
        signal=sum(np.sin(TAU*f*r*t)*np.exp(-t*d)*a for r,d,a in [(1,2,.8),(2.01,4,.3),(3.98,6,.16),(5.4,8,.05)])
    elif kind=='flute':
        phase=TAU*f*t+.04*np.sin(TAU*5*t)
        signal=(np.sin(phase)+.17*np.sin(2*phase)+.04*np.sin(3*phase))*(.88+.12*np.sin(TAU*2*t))
    elif kind=='choir':
        signal=sum(np.sin(TAU*f*h*t+.02*np.sin(TAU*(3+h*.1)*t))/h**1.8 for h in range(1,7))
    else:
        signal=sum((np.sin(TAU*f*h*t)+np.sin(TAU*f*1.003*h*t))*.5/h**1.6 for h in range(1,8))
    return signal*attack*np.clip(release,0,1)

def add(buffer,signal,start,gain=1,pan=0):
    index=(np.arange(len(signal))+round(start*RATE))%len(buffer)
    np.add.at(buffer[:,0],index,signal*gain*np.sqrt((1-pan)/2))
    np.add.at(buffer[:,1],index,signal*gain*np.sqrt((1+pan)/2))

def write(path,buffer,loop=False,peak=.78):
    if buffer.ndim==1: buffer=np.column_stack((buffer,buffer))
    if loop:
        # Circular room reflections retain the end of every note through the seam.
        dry=buffer.copy()
        for delay,gain in [(.127,.12),(.233,.09),(.389,.065),(.577,.04)]:
            buffer+=np.roll(dry[:,::-1],round(delay*RATE),axis=0)*gain
    else:
        # Tail-safe reflections without wrapping transient attacks to the beginning.
        dry=buffer.copy()
        for delay,gain in [(.061,.16),(.137,.1),(.229,.06)]:
            shift=round(delay*RATE)
            if shift<len(buffer): buffer[shift:]+=dry[:-shift,::-1]*gain
        n=min(round(.025*RATE),len(buffer)//4)
        buffer[:n]*=np.linspace(0,1,n)[:,None];buffer[-n:]*=np.linspace(1,0,n)[:,None]
    buffer*=peak/max(np.max(np.abs(buffer)),.001)
    pcm=(np.clip(buffer,-1,1)*32767).astype('<i2')
    path.parent.mkdir(parents=True,exist_ok=True)
    with wave.open(str(path),'wb') as w: w.setparams((2,2,RATE,0,'NONE','not compressed'));w.writeframes(pcm.tobytes())
    return {'file':str(path.relative_to(ROOT)),'seconds':round(len(buffer)/RATE,3),'peak':round(float(np.max(np.abs(buffer))),3),'rms':round(float(np.sqrt(np.mean(buffer**2))),4),'seam_delta':round(float(np.max(np.abs(buffer[0]-buffer[-1]))),4),'sha256':hashlib.sha256(path.read_bytes()).hexdigest()}

TRACKS=[
 ('Menu','The Forbidden Admission',78,62,[0,2,3,5,7,8,11],[0,5,3,4],['celesta','choir'],[7,9,7,4,5,4,2,0,2,4,7,6,5,4,2,0]),
 ('Witchwood','Dawn Among the Wards',100,64,[0,2,3,5,7,9,10],[0,3,5,4],['flute','strings'],[0,2,4,6,5,4,2,1,2,3,5,4,2,1,0,4]),
 ('ObsidianHall','Sunlit Grimoire',84,57,[0,2,3,5,7,8,10],[0,5,2,4],['harp','strings'],[4,6,5,4,2,3,1,0,3,5,6,4,2,1,2,0]),
 ('AstralTower','Constellations After Curfew',72,60,[0,2,3,5,7,8,10],[0,5,3,6],['celesta','choir'],[7,4,6,3,5,2,4,0,6,5,3,4,2,1,4,0])]
manifest={'sample_rate':RATE,'channels':2,'composition':'Original procedural chamber-fantasy music; harp, celesta, flute, strings and choir synthesis. No third-party samples.','music':[],'sfx':[]}
for key,title,bpm,tonic,scale,progression,instruments,melody in TRACKS:
    beat=60/bpm; length=32*beat; mix=np.zeros((round(length*RATE),2))
    def note(degree,octave=0): return tonic+scale[degree%7]+12*(degree//7+octave)
    for bar in range(8):
        root=progression[bar%4];start=bar*4*beat
        for j,degree in enumerate([root,root+2,root+4]):
            add(mix,voice(note(degree,-1),4.2*beat,instruments[1]),start,.09,(-.4+j*.4))
        add(mix,voice(note(root,-2),3.9*beat,'strings'),start,.12)
        for step in range(8):
            degree=root+[0,4,2,7,4,2,4,2][step]
            add(mix,voice(note(degree),1.15,'harp'),start+step*.5*beat,.14,(-.5 if step%2 else .5))
        for step in range(2):
            degree=melody[bar*2+step]
            add(mix,voice(note(degree,1),1.8*beat,instruments[0]),start+step*2*beat,.16,.1)
        if bar%2==0: add(mix,voice(note(root+7,1),2.5,'celesta'),start,.075,-.3)
    record=write(ROOT/'Assets/Resources/Audio/Music'/f'{key}.wav',mix,True,.68)
    record.update(title=title,bpm=bpm,voices=instruments+['harp','celesta']);manifest['music'].append(record)

# Transient sound identities: crystalline menus, ward impacts, airy casts and void falls.
for key,duration in [('select',.24),('confirm',.65),('punch',.27),('hit',.36),('cast',.8),('power',1.25),('jump',.38),('step',.15),('fall',1.15),('win',1.7),('bell',.8),('block',.48)]:
    t=np.arange(round(duration*RATE))/RATE; mix=np.zeros((len(t),2)); noise=rng.normal(0,1,len(t))
    if key in ('select','confirm','win','bell'):
        notes={'select':[86,93],'confirm':[74,81,86],'win':[62,69,74,77,81],'bell':[86]}[key]
        for i,note in enumerate(notes):
            start=i*(.07 if key=='select' else .12)
            signal=voice(note,max(.03,duration-start),'celesta')
            # No loop/wrapping for effects: keep each strike inside its own tail.
            n=min(len(signal),len(t)-round(start*RATE));offset=round(start*RATE)
            mix[offset:offset+n]+=signal[:n,None]*np.array([.7,.7])
    elif key in ('punch','hit','step','block'):
        decay={'punch':24,'hit':19,'step':45,'block':12}[key]
        signal=noise*np.exp(-t*decay)*(.18 if key=='step' else .24)
        if key!='punch': signal+=np.sin(TAU*(75 if key in ('hit','step') else 690)*t)*np.exp(-t*decay)*.5
        if key=='block': signal+=.3*np.sin(TAU*1384*t)*np.exp(-t*9)
        mix+=signal[:,None]*np.array([.75,.65])
    else:
        start,end={'cast':(360,1250),'power':(85,880),'jump':(240,660),'fall':(650,38)}[key]
        freq=start*(end/start)**(t/duration);phase=TAU*np.cumsum(freq)/RATE
        env=np.sin(np.pi*np.minimum(t/duration,1))**1.5
        signal=(np.sin(phase)+.28*np.sin(phase*2.01)+noise*.08)*env*.55
        if key in ('cast','power'): signal+=np.sin(TAU*1760*t)*np.sin(TAU*7*t)**8*np.exp(-t*2)*.13
        if key=='power': signal+=np.sin(TAU*55*t)*env*.35
        mix+=signal[:,None]*np.array([.65,.75])
    manifest['sfx'].append(write(ROOT/'Assets/Resources/Audio/SFX'/f'{key}.wav',mix,False,.7 if key in ('power','win','fall') else .6))
(ROOT/'Docs/audio_manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('Composed 4 distinct wizard-academy loops and 12 magical SFX; saved audio metrics/credits in Docs/audio_manifest.json.')
