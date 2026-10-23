"""Original, deterministic pixel art and chiptune assets. Python standard library only."""
from pathlib import Path
import math, random, struct, wave, zlib
ROOT = Path(__file__).resolve().parents[1] / 'Assets' / 'Resources'
NAMES = ['Lyra','Mira','Selene','Iris','Alden','Rowan','Orion','Dorian']
COLORS = [(255,108,64),(101,224,255),(185,140,255),(121,235,151),(255,211,93),(97,207,175),(108,151,255),(242,126,190)]
SKINS = [(235,174,133),(166,104,78),(222,177,148),(114,73,61),(196,136,93),(236,187,147),(136,88,72),(207,157,116)]
HAIR = [(112,41,48),(217,232,240),(48,36,72),(39,63,54),(218,186,112),(76,44,37),(28,34,64),(215,207,225)]
class Canvas:
    def __init__(self,w,h,bg=(0,0,0,0)): self.w,self.h=w,h; self.p=[bg]*(w*h)
    def dot(self,x,y,c):
        x,y=int(x),int(y)
        if 0<=x<self.w and 0<=y<self.h: self.p[y*self.w+x]=tuple(c)+(255,) if len(c)==3 else c
    def rect(self,x,y,w,h,c):
        for yy in range(int(y),int(y+h)):
            for xx in range(int(x),int(x+w)): self.dot(xx,yy,c)
    def line(self,x,y,xx,yy,c):
        steps=max(abs(int(xx-x)),abs(int(yy-y)),1)
        for i in range(steps+1): self.dot(x+(xx-x)*i/steps,y+(yy-y)*i/steps,c)
    def disk(self,x,y,r,c):
        for yy in range(int(y-r),int(y+r+1)):
            for xx in range(int(x-r),int(x+r+1)):
                if (xx-x)**2+(yy-y)**2<=r*r: self.dot(xx,yy,c)
    def save(self,path):
        path.parent.mkdir(parents=True,exist_ok=True)
        raw=b''.join(b'\0'+bytes(v for p in self.p[y*self.w:(y+1)*self.w] for v in p) for y in range(self.h))
        def chunk(t,d): return struct.pack('>I',len(d))+t+d+struct.pack('>I',zlib.crc32(t+d)&0xffffffff)
        path.write_bytes(b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>IIBBBBB',self.w,self.h,8,6,0,0,0))+chunk(b'IDAT',zlib.compress(raw))+chunk(b'IEND',b''))

def fighter(c,i,ox,oy,row,frame):
    col,skin,hair=COLORS[i],SKINS[i],HAIR[i]; outline=(15,15,31); robe=tuple(int(v*.33) for v in col)
    bob=[0,0,-1,0][frame]; x=ox+30; y=oy+12+bob
    if row==4: # Knockout: progressive fall into a horizontal resting pose.
        if frame<2: y+=frame*6; x-=frame*3
        else:
            c.rect(ox+12,oy+45,39,10,outline); c.rect(ox+14,oy+46,24,7,robe); c.rect(ox+39,oy+45,9,9,skin)
            c.rect(ox+40,oy+43,10,3,hair); c.line(ox+43,oy+48,ox+46,oy+50,outline); return
    if row==3: x-=frame*2
    stride=[-4,0,4,0][frame] if row==1 else 0
    # Boots, outlined legs, coat, gold belt and trim.
    c.rect(x-8+stride,y+31,6,14,outline); c.rect(x+3-stride,y+31,6,14,outline)
    c.rect(x-9+stride,y+42,8,3,(34,32,51)); c.rect(x+3-stride,y+42,8,3,(34,32,51))
    c.rect(x-10,y+15,21,23,outline); c.rect(x-8,y+16,17,20,robe)
    c.rect(x-7,y+17,3,17,col); c.rect(x+5,y+17,2,18,col)
    c.rect(x-8,y+28,17,2,(220,185,116)); c.rect(x-1,y+15,3,9,(237,226,208))
    # Heads and eight hair silhouettes.
    c.rect(x-7,y-1,15,15,outline); c.rect(x-6,y,13,12,skin); c.rect(x-7,y-2,15,5,hair)
    c.rect(x-7,y+2,3,9,hair); c.rect(x+1,y+6,2,2,outline); c.rect(x+6,y+6,1,2,outline)
    if i<4:
        c.rect(x-10,y+1,4,19 if i!=1 else 11,hair)
        if i==0: c.rect(x-13,y+5,4,13,hair)
        if i==2: c.rect(x-9,y-5,17,3,col); c.rect(x-5,y-9,10,5,robe)
        if i==3: c.disk(x-8,y,5,hair); c.dot(x-9,y-3,col)
    else:
        c.rect(x-5,y-4,4,3,hair); c.rect(x+1,y-3,4,3,hair)
        if i==6: c.rect(x-7,y+5,14,2,col)
        if i==7: c.rect(x-8,y+11,3,9,(190,185,219))
    reach=([0,10,15,4][frame] if row in (2,5,8) else [3,7,10,6][frame] if row in (6,7) else 0)
    c.rect(x+8,y+17,5+reach,5,outline); c.rect(x+9,y+18,4+reach,3,skin)
    c.rect(x-12,y+17,4,13,outline); c.rect(x-11,y+19,3,9,skin)
    # Character weapon: wand, ice blade, staff, vine, hammer, spear, orb, scythe.
    wx=x+12+reach; wy=y+17
    if i in (0,2,5,7):
        c.line(wx,wy+5,wx+3,wy-13,(192,150,99)); c.disk(wx+3,wy-13,2,col)
        if i==7: c.line(wx+3,wy-13,wx+10,wy-8,col)
    elif i in (1,3): c.line(wx,wy,wx+5,wy-12,col); c.line(wx+1,wy,wx+6,wy-12,col)
    elif i==4: c.rect(wx,wy-10,2,14,(159,115,72)); c.rect(wx-3,wy-12,9,5,col)
    else: c.disk(wx+2,wy-4,3,col)
    if row in (5,6,7):
        for k in range(8):
            a=k*math.pi/4+frame*.6; c.dot(x+math.cos(a)*(18+frame),y+20+math.sin(a)*23,col)
    if row==3: c.line(x-3,y+5,x,y+8,(255,241,230))

for i,name in enumerate(NAMES):
    sheet=Canvas(256,640)
    for row in range(10):
        for f in range(4): fighter(sheet,i,f*64,row*64,row,f)
    sheet.save(ROOT/'Characters'/name/'sheet.png')
    portrait=Canvas(64,64,(24,24,43,255))
    portrait.disk(32,30,28,tuple(int(v*.3) for v in COLORS[i])); fighter(portrait,i,0,5,0,0)
    portrait.save(ROOT/'Characters'/name/'avatar.png')
    fx=Canvas(256,32)
    for f in range(8):
        for k in range(14):
            a=k*math.tau/14+f*.35; r=4+f*1.5+(k%3)
            x=f*32+16+math.cos(a)*r; y=16+math.sin(a)*r
            fx.line(f*32+16,16,x,y,tuple(int(v*.5) for v in COLORS[i]))
            if i%3==0: fx.rect(x,y,2,3,COLORS[i])
            elif i%3==1: fx.line(x-2,y,x+2,y,COLORS[i]); fx.line(x,y-2,x,y+2,COLORS[i])
            else: fx.disk(x,y,2,COLORS[i])
        fx.disk(f*32+16,16,3,(255,247,215))
    fx.save(ROOT/'Effects'/(name+'.png'))

for stage in range(3):
    rng=random.Random(82+stage); c=Canvas(480,270)
    for y in range(270): c.rect(0,y,480,1,(12+y//27,14+y//24,29+y//13))
    c.disk(375,49,25,(160,161,184)); c.disk(382,43,23,(28,30,51))
    for k in range(90): c.dot(rng.randrange(480),rng.randrange(160),(95,107,140))
    if stage==0:
        for x in range(-20,500,38):
            h=rng.randrange(100,190); c.rect(x,200-h,9,h,(20,28,38))
            for j in range(5):
                yy=200-h+j*18; c.line(x+4,yy+32,x-24,yy,(27,42,47)); c.line(x+4,yy+32,x+32,yy-7,(27,42,47))
        for x in range(0,480,73):
            c.rect(x,35,12,175,(29,40,45)); c.line(x+4,90,x-31,45,(29,40,45)); c.line(x+6,115,x+46,61,(29,40,45))
        for k in range(90): c.dot(rng.randrange(480),rng.randrange(90,205),(88,145,132))
    else:
        for x in range(0,480,60):
            c.rect(x,40,13,168,(48,43,65)); c.rect(x-3,40,19,6,(78,66,90)); c.rect(x-3,194,19,7,(78,66,90))
            c.rect(x+20,58,28,105,(13,19,37)); c.rect(x+23,61,22,98,(55,45,88) if stage==1 else (30,57,76))
            c.rect(x+33,59,2,103,(109,87,127)); c.rect(x+21,95,26,3,(109,87,127))
            if stage==1:
                for yy in range(140,191,16):
                    c.rect(x+16,yy,39,3,(72,53,61))
                    for xx in range(x+17,x+53,5): c.rect(xx,yy-10,3,10,(75+rng.randrange(30),51,71+rng.randrange(25)))
        if stage==2:
            for r in range(25,80,10):
                for a in range(360): c.dot(240+math.cos(a*math.pi/180)*r,115+math.sin(a*math.pi/180)*r,(64,92,120))
            c.line(170,115,310,115,(191,158,101)); c.line(240,45,240,185,(191,158,101))
    c.rect(0,207,480,63,(38,35,49)); c.rect(0,207,480,3,(125,105,111))
    for y in range(214,270,13):
        c.rect(0,y,480,1,(64,56,69))
        for x in range((y%2)*20,480,40): c.rect(x,y,1,13,(64,56,69))
    for x in (37,438):
        c.rect(x,176,5,32,(91,77,91)); c.disk(x+2,173,5,(245,193,107)); c.disk(x+2,172,2,(255,242,199))
    c.save(ROOT/'Stages'/(['Witchwood','ObsidianHall','AstralTower'][stage]+'.png'))

def sound(path,notes,duration,volume=.17):
    rate=22050; samples=[]
    for n in range(int(rate*duration)):
        t=n/rate; beat=duration/len(notes); f=notes[min(int(t/beat),len(notes)-1)]; p=(t%beat)/beat
        env=min(p*25,1)*min((1-p)*12,1)
        value=(1 if math.sin(math.tau*f*t)>0 else -1)*.6+math.sin(math.tau*f*.5*t)*.4
        samples.append(int(value*env*volume*32767))
    path.parent.mkdir(parents=True,exist_ok=True)
    with wave.open(str(path),'wb') as w: w.setparams((1,2,rate,0,'NONE','not compressed')); w.writeframes(struct.pack('<'+'h'*len(samples),*samples))
for name,notes in {'select':[440,660],'confirm':[440,554,880],'punch':[160,90],'hit':[100,65],'cast':[330,440,660],'power':[220,330,440,660],'jump':[180,300],'step':[75],'fall':[400,200,100,50],'win':[330,440,554,660],'bell':[880,660],'block':[220,170]}.items():
    sound(ROOT/'Audio'/'SFX'/(name+'.wav'),notes,.12 if name=='step' else .32 if name not in ('win','power') else .8)
for i,name in enumerate(['Menu','Witchwood','ObsidianHall','AstralTower']):
    base=[220,261.63,329.63,293.66,220,196,246.94,164.81]
    sound(ROOT/'Audio'/'Music'/(name+'.wav'),[x*(1+i*.125) for x in base*4],16,.085)
print('Generated 8 sheets (320 frames), 8 avatars, 8 effect sheets, 3 arenas, and 16 WAV files.')
