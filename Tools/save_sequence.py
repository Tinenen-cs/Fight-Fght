"""Save one generated four-frame source sequence without painting changes."""
from pathlib import Path
import sys
from PIL import Image
root=Path(__file__).resolve().parents[1]
source,key,pose=sys.argv[1:]
assert key in ['Lyra','Mira','Selene','Iris','Alden','Rowan','Orion','Dorian']
assert pose in ['Idle','Walk','Punch','Hurt','Knockout','Combo','Special','Power','Push','Jump']
im=Image.open(source).convert('RGBA')
assert im.getextrema()[3][0]==0, 'Generated sequence lacks transparent pixels'
im=im.resize((512,512),Image.Resampling.NEAREST)
for row in range(2):
 for col in range(2): assert im.crop((col*256,row*256,(col+1)*256,(row+1)*256)).getbbox(), 'Empty frame'
dest=root/'Assets/Art/CharacterSequences'/key/(pose+'.png'); dest.parent.mkdir(parents=True,exist_ok=True); im.save(dest)
print(key+'/'+pose+' saved')
poses=['Idle','Walk','Punch','Hurt','Knockout','Combo','Special','Power','Push','Jump']
keys=['Lyra','Mira','Selene','Iris','Alden','Rowan','Orion','Dorian']
rows=[]; count=0
for character in keys:
    done=[p for p in poses if (root/'Assets/Art/CharacterSequences'/character/(p+'.png')).exists()]
    count+=len(done)
    rows.append('| '+character+' | '+str(len(done))+' / 10 | '+', '.join(done)+' |')
report='# Animation generation progress\n\n'+str(count)+' / 80 four-frame sequences saved.\n\n| Internal character ID | Saved sequences | Actions |\n| --- | --- | --- |\n'+'\n'.join(rows)+'\n'
import tempfile,os
with tempfile.NamedTemporaryFile(mode='w',dir=root/'Docs',prefix='.art-progress-',delete=False) as f:
    f.write(report); temporary=f.name
os.replace(temporary,root/'Docs/ART_PROGRESS.md')
