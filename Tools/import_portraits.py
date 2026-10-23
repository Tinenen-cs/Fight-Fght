"""Slice authored four-reaction head sheets for selection and live HUD icons.
Requires Pillow. Pixel sampling/cropping only; original artwork remains committed.
"""
from pathlib import Path
import json
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
manifest=json.loads((ROOT/'Docs/portrait_art_manifest.json').read_text())
contact=Image.new('RGBA',(512,256),'#111320')
for i,entry in enumerate(manifest['portraits']):
    im=Image.open(ROOT/entry['source']).convert('RGBA')
    assert im.getextrema()[3][0]==0,entry['key']+' must be transparent'
    atlas=Image.new('RGBA',(512,512))
    for frame in range(4):
        col,row=frame%2,frame//2
        cell=im.crop((col*im.width//2,row*im.height//2,(col+1)*im.width//2,(row+1)*im.height//2))
        bounds=cell.getbbox(); assert bounds,(entry['key'],frame)
        # Keep full head and hat inside each HUD cell, with a consistent safe margin.
        cell=cell.crop(bounds); cell.thumbnail((224,224),Image.Resampling.LANCZOS)
        atlas.alpha_composite(cell,(col*256+(256-cell.width)//2,row*256+(256-cell.height)//2))
    folder=ROOT/'Assets/Resources/Characters'/entry['key']
    atlas.save(folder/'portraits.png')
    avatar=atlas.crop((0,0,256,256));avatar.save(folder/'avatar.png')
    contact.alpha_composite(avatar.resize((128,128),Image.Resampling.LANCZOS),((i%4)*128,(i//4)*128))
contact.convert('RGB').save(ROOT/'Docs/Previews/09-head-icons.png')
print('Imported eight head portraits and 32 reaction frames.')
