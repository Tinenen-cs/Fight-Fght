using UnityEngine;
namespace Hexbound
{
    // Keep HUD bounds separate from the complete portal artwork, on every arena.
    public static class CombatHudLayout
    {
        public const float GroundY=557, ControlsTop=564;
        public static Rect Portal(float x,float size=144) => new Rect(x-size/2,GroundY-10-size/2,size,size);
        // Punch west, shove south, spell north, power east; mirrored for P2.
        public static Rect Ability(int side,int action)
        {
            Vector2[] offset={new Vector2(-88,0),new Vector2(0,40),new Vector2(0,-40),new Vector2(88,0)};
            float x=side==0?360:920;
            return new Rect(x+offset[action].x*(side==0?1:-1)-36,640+offset[action].y-36,72,72);
        }
    }
}
