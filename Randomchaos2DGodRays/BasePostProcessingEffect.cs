using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randomchaos2DGodRays
{
    public class BasePostProcessingEffect
    {
        public bool Enabled = true;
        protected Game Game;
        public Vector2 HalfPixel;
        public Texture2D lastScene;
        public Texture2D orgScene;
        protected List<BasePostProcess> postProcesses = new List<BasePostProcess>();

        public BasePostProcessingEffect(Game game)
        {
            Game = game;
        }

        public void AddPostProcess(BasePostProcess postProcess)
        {
            postProcesses.Add(postProcess);
        }

        public virtual void Draw(GameTime gameTime, Texture2D scene)
        {
            if (!Enabled)
            {
                return;
            }

            orgScene = scene;
            var count = postProcesses.Count;
            lastScene = null;
            for (var i = 0; i < count; i++)
            {
                if (!postProcesses[i].Enabled)
                {
                    continue;
                }

                postProcesses[i].HalfPixel = HalfPixel;
                postProcesses[i].orgBuffer = orgScene;
                if (postProcesses[i].newScene == null)
                {
                    postProcesses[i].newScene = new RenderTarget2D(Game.GraphicsDevice,
                        Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2, false,
                        SurfaceFormat.Color, DepthFormat.None);
                }

                Game.GraphicsDevice.SetRenderTarget(postProcesses[i].newScene);
                if (lastScene == null)
                {
                    lastScene = orgScene;
                }

                postProcesses[i].BackBuffer = lastScene;
                Game.GraphicsDevice.Textures[0] = postProcesses[i].BackBuffer;
                postProcesses[i].Draw(gameTime);
                Game.GraphicsDevice.SetRenderTarget(null);
                lastScene = postProcesses[i].newScene;
            }

            if (lastScene == null)
            {
                lastScene = scene;
            }
        }
    }
}