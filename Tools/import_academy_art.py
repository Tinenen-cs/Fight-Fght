"""Normalize committed scene, VFX and interface art. Requires Pillow.
All painting comes from the built-in image tool; this script only samples pixels.
Fighters are handled separately by assemble_sequences.py.
"""
from pathlib import Path
import json
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
manifest=json.loads((ROOT/'Docs/academy_art_manifest.json').read_text())
def normalize(im,size,colors=128):
    return im.convert('RGBA').resize(size,Image.Resampling.LANCZOS)
for entry in manifest['assets']:
    source=ROOT/entry['source']
    if not source.exists():
        raise FileNotFoundError('Missing committed art source: '+str(source))
    im=Image.open(source).convert('RGBA'); key=entry['key']; dest=ROOT/entry['runtime']
    dest.parent.mkdir(parents=True,exist_ok=True)
    if key.startswith('stage_'):
        normalize(im,(1280,720),256).convert('RGB').save(dest)
    elif key.startswith('effect_'):
        atlas=normalize(im,(1024,512))
        assert atlas.getextrema()[3][0]==0,(key,'missing transparency')
        for row in range(2):
            for col in range(4): assert atlas.crop((col*256,row*256,(col+1)*256,(row+1)*256)).getbbox(),(key,row,col)
        atlas.save(dest)
    elif key.startswith('trap_'):
        normalize(im,(512,512)).save(dest)
    else:
        bounds=im.getbbox()
        if bounds: im=im.crop(bounds)
        normalize(im,(1024,512) if key=='ui_panel' else (1024,192)).save(dest)
print('Imported',len(manifest['assets']),'scene, effect, UI and trap assets.')
