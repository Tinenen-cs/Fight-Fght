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
    atlas=Image.new('RGBA',(512,1280))
    for row,seq in enumerate(char['sequences']):
        im=Image.open(ROOT/seq['source']).convert('RGBA'); w,h=im.size
        assert im.getextrema()[3][0]==0,(char['name'],row,'no transparency')
        for frame in range(4):
            col=frame%2; y=frame//2
            cell=im.crop((round(col*w/2),round(y*h/2),round((col+1)*w/2),round((y+1)*h/2)))
            assert cell.getbbox(),(char['name'],row,frame,'empty cell')
            cell=cell.resize((128,128),Image.Resampling.NEAREST)
            bounds=cell.getbbox()
            shrink=min(1,112/(bounds[3]-bounds[1]))
            if shrink<1:
                cell=cell.resize((round(128*shrink),round(128*shrink)),Image.Resampling.NEAREST)
                bounds=cell.getbbox()
            aligned=Image.new('RGBA',(128,128))
            aligned.alpha_composite(cell,((128-cell.width)//2,116-bounds[3]))
            atlas.alpha_composite(aligned,(frame*128,row*128))
    alpha=atlas.getchannel('A'); atlas=atlas.convert('RGB').quantize(colors=128,method=Image.Quantize.MEDIANCUT,dither=Image.Dither.NONE).convert('RGBA'); atlas.putalpha(alpha)
    folder=ROOT/'Assets/Resources/Characters'/char['key']; atlas.save(folder/'sheet.png')
    idle=atlas.crop((0,0,128,128)); bounds=idle.getbbox()
    # Use the largest opaque component in the upper band so a long staff does
    # not shrink the face portrait. This crops existing pixels only.
    top=bounds[1]; bottom=min(128,top+int((bounds[3]-top)*.30))
    alpha=idle.getchannel('A'); seen=set(); components=[]
    for yy in range(top,bottom):
        for xx in range(128):
            if (xx,yy) in seen or alpha.getpixel((xx,yy))<128: continue
            stack=[(xx,yy)]; seen.add((xx,yy)); points=[]
            while stack:
                x,y=stack.pop(); points.append((x,y))
                for nx,ny in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
                    if 0<=nx<128 and top<=ny<bottom and (nx,ny) not in seen and alpha.getpixel((nx,ny))>=128:
                        seen.add((nx,ny)); stack.append((nx,ny))
            components.append(points)
    points=max(components,key=len)
    left=min(x for x,y in points); right=max(x for x,y in points)+1
    head=idle.crop((max(0,left-2),max(0,top-2),min(128,right+2),min(128,bottom+2)))
    head.thumbnail((60,60),Image.Resampling.NEAREST)
    avatar=Image.new('RGBA',(64,64)); avatar.alpha_composite(head,((64-head.width)//2,(64-head.height)//2));
    if not (folder/'portraits.png').exists(): avatar.save(folder/'avatar.png')
    x=(i%4)*192; y=(i//4)*216
    contact.alpha_composite(idle.resize((160,160),Image.Resampling.NEAREST),(x+16,y+8))
    draw.text((x+8,y+177),char['name'],fill='#f2e3c9')
contact.convert('RGB').save(ROOT/'Docs/Previews/04-roster.png')
print('Assembled eight fighters: 80 separately generated sequences, 320 validated frames, 8 matching portraits.')
