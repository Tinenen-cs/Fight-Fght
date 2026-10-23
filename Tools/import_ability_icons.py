"""Import separately authored 2x2 diamond ability icons. Requires Pillow."""
from pathlib import Path
import json
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
manifest=json.loads((ROOT/'Docs/ability_icon_manifest.json').read_text())
contact=Image.new('RGBA',(1024,512),'#111320')
for i,entry in enumerate(manifest['icons']):
    im=Image.open(ROOT/entry['source']).convert('RGBA')
    assert im.getextrema()[3][0]==0,(entry['key'],'missing alpha')
    atlas=Image.new('RGBA',(512,512))
    for frame in range(4):
        col,row=frame%2,frame//2
        tile=im.crop((col*im.width//2,row*im.height//2,(col+1)*im.width//2,(row+1)*im.height//2))
        assert tile.getbbox(),(entry['key'],frame)
        tile=tile.crop(tile.getbbox());tile.thumbnail((240,240),Image.Resampling.LANCZOS)
        atlas.alpha_composite(tile,(col*256+(256-tile.width)//2,row*256+(256-tile.height)//2))
    atlas.save(ROOT/'Assets/Resources/Characters'/entry['key']/'abilities.png')
    contact.alpha_composite(atlas.resize((256,256),Image.Resampling.LANCZOS),((i%4)*256,(i//4)*256))
contact.convert('RGB').save(ROOT/'Docs/Previews/11-ability-icons.png')
print('Imported 32 dedicated diamond ability icons for eight fighters.')
