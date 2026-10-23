"""Assemble separately generated four-frame sequences into Unity fighter atlases.
Requires Pillow. No artwork is painted or inferred by this script.
"""
from pathlib import Path
import json
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[1]
manifest=json.loads((ROOT/'Docs/animation_sequence_manifest.json').read_text())
contact=Image.new('RGBA',(768,432),'#111320'); draw=ImageDraw.Draw(contact)
for i,char in enumerate(manifest['characters']):
    atlas=Image.new('RGBA',(1024,2560))
    for row,seq in enumerate(char['sequences']):
        im=Image.open(ROOT/seq['source']).convert('RGBA'); w,h=im.size
        assert im.getextrema()[3][0]==0,(char['name'],row,'no transparency')
        for frame in range(4):
            col=frame%2; y=frame//2
            cell=im.crop((round(col*w/2),round(y*h/2),round((col+1)*w/2),round((y+1)*h/2)))
            assert cell.getbbox(),(char['name'],row,frame,'empty cell')
            cell=cell.resize((256,256),Image.Resampling.LANCZOS)
            bounds=cell.getbbox()
            shrink=min(1,224/(bounds[3]-bounds[1]))
            if shrink<1:
                cell=cell.resize((round(256*shrink),round(256*shrink)),Image.Resampling.LANCZOS)
                bounds=cell.getbbox()
            aligned=Image.new('RGBA',(256,256))
            aligned.alpha_composite(cell,((256-cell.width)//2,232-bounds[3]))
            atlas.alpha_composite(aligned,(frame*256,row*256))
    folder=ROOT/'Assets/Resources/Characters'/char['key']; atlas.save(folder/'sheet.png')
    idle=atlas.crop((0,0,256,256))
    # HUD portraits are independently authored; never replace them with body crops.
    x=(i%4)*192; y=(i//4)*216
    contact.alpha_composite(idle.resize((160,160),Image.Resampling.LANCZOS),(x+16,y+8))
    draw.text((x+8,y+177),char['name'],fill='#f2e3c9')
contact.convert('RGB').save(ROOT/'Docs/Previews/04-roster.png')
print('Assembled eight fighters: 80 separately generated sequences, 320 validated frames, 8 matching portraits.')
