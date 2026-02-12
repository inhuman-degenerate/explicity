using Verse;
using HarmonyLib;
using UnityEngine;

namespace Explicity
{
    public class MyGameComponent : GameComponent
    {
        public float height = 0;
        public float width = 0;
        public MyGameComponent(Game game) { }

        public override void GameComponentUpdate()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                height += 0.02f;
                Log.Message(height);
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                height -= 0.02f;
                Log.Message(height);
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                width += 0.02f;
                Log.Message(width);
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                width -= 0.02f;
                Log.Message(width);
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                width = 0f;
                height = 0f;
            }
        }
    }
}
