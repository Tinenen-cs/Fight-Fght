using UnityEngine;
namespace Hexbound
{
    // Keep HUD bounds separate from the complete portal artwork, on every arena.
    public static class CombatHudLayout
    {
        public const float GroundY=557, ControlsTop=624;
        public static Rect Portal(float x) => new Rect(x-72,GroundY-82,144,144);
        // Punch west, shove south, spell north, power east; mirrored for P2.
        public static Rect Ability(int side,int action)
        {
            Vector2[] offset={new Vector2(-54,0),new Vector2(0,24),new Vector2(0,-24),new Vector2(54,0)};
            float x=side==0?145:1135;
            return new Rect(x+offset[action].x*(side==0?1:-1)-24,672+offset[action].y-24,48,48);
        }
    }
}
