/*
  Rogue Legacy Enhanced

  This project is based on modified disassembly of Rogue Legacy's engine, with permission to do so by its creators.
  Therefore, former creators copyright notice applies to original disassembly. 

  Disassembled source Copyright(C) 2011-2015, Cellar Door Games Inc.
  Rogue Legacy(TM) is a trademark or registered trademark of Cellar Door Games Inc. All Rights Reserved.
*/

using System;
using DS2DEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueCastle
{
    public class NpcObj : ObjContainer
    {
        private SpriteObj m_talkIcon;
        public bool CanTalk { get; set; }

        public bool IsTouching
        {
            get { return m_talkIcon.Visible; }
        }

        public NpcObj(string spriteName) : base(spriteName)
        {
            CanTalk = true;
            m_talkIcon = new SpriteObj("ExclamationBubble_Sprite");
            m_talkIcon.Scale = new Vector2(2f, 2f);
            m_talkIcon.Visible = false;
            m_talkIcon.OutlineWidth = 2;
            OutlineWidth = 2;
        }

        public void Update(GameTime gameTime, PlayerObj player)
        {
            bool flag = false;
            if (Flip == SpriteEffects.None && player.X > X)
            {
                flag = true;
            }
            if (Flip != SpriteEffects.None && player.X < X)
            {
                flag = true;
            }
            if (player != null &&
                CollisionMath.Intersects(player.TerrainBounds,
                    new Rectangle(Bounds.X - 50, Bounds.Y, Bounds.Width + 100, Bounds.Height)) && flag &&
                player.Flip != Flip && CanTalk)
            {
                m_talkIcon.Visible = true;
            }
            else
            {
                m_talkIcon.Visible = false;
            }
            if (Flip == SpriteEffects.None)
            {
                m_talkIcon.Position = new Vector2(Bounds.Left - m_talkIcon.AnchorX,
                    Bounds.Top - m_talkIcon.AnchorY + (float) Math.Sin(Game.TotalGameTime*20f)*2f);
                return;
            }
            m_talkIcon.Position = new Vector2(Bounds.Right + m_talkIcon.AnchorX,
                Bounds.Top - m_talkIcon.AnchorY + (float) Math.Sin(Game.TotalGameTime*20f)*2f);
        }

        public override void Draw(Camera2D camera)
        {
            if (Flip == SpriteEffects.None)
            {
                m_talkIcon.Flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                m_talkIcon.Flip = SpriteEffects.None;
            }
            base.Draw(camera);
            m_talkIcon.Draw(camera);
        }

        protected override GameObj CreateCloneInstance()
        {
            return new NpcObj(SpriteName);
        }

        protected override void FillCloneInstance(object obj)
        {
            base.FillCloneInstance(obj);
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                m_talkIcon.Dispose();
                m_talkIcon = null;
                base.Dispose();
            }
        }
    }
}