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
    public class ProjectileIconPool : IDisposable
    {
        private readonly int m_poolSize;
        private ProjectileManager m_projectileManager;
        private DS2DPool<ProjectileIconObj> m_resourcePool;
        private RCScreenManager m_screenManager;

        public ProjectileIconPool(int poolSize, ProjectileManager projectileManager, RCScreenManager screenManager)
        {
            m_poolSize = poolSize;
            m_resourcePool = new DS2DPool<ProjectileIconObj>();
            m_projectileManager = projectileManager;
            m_screenManager = screenManager;
        }

        public bool IsDisposed { get; private set; }

        public int ActiveTextObjs
        {
            get { return m_resourcePool.NumActiveObjs; }
        }

        public int TotalPoolSize
        {
            get { return m_resourcePool.TotalPoolSize; }
        }

        public int CurrentPoolSize
        {
            get { return TotalPoolSize - ActiveTextObjs; }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Console.WriteLine("Disposing Projectile Icon Pool");
                m_resourcePool.Dispose();
                m_resourcePool = null;
                IsDisposed = true;
                m_projectileManager = null;
                m_screenManager = null;
            }
        }

        public void Initialize()
        {
            for (var i = 0; i < m_poolSize; i++)
            {
                var projectileIconObj = new ProjectileIconObj();
                projectileIconObj.Visible = false;
                projectileIconObj.ForceDraw = true;
                projectileIconObj.TextureColor = Color.White;
                m_resourcePool.AddToPool(projectileIconObj);
            }
        }

        public void AddIcon(ProjectileObj projectile)
        {
            var projectileIconObj = m_resourcePool.CheckOut();
            projectileIconObj.Visible = true;
            projectileIconObj.ForceDraw = true;
            projectileIconObj.AttachedProjectile = projectile;
            projectile.AttachedIcon = projectileIconObj;
        }

        public void DestroyIcon(ProjectileObj projectile)
        {
            var attachedIcon = projectile.AttachedIcon;
            attachedIcon.Visible = false;
            attachedIcon.Rotation = 0f;
            attachedIcon.TextureColor = Color.White;
            attachedIcon.Opacity = 1f;
            attachedIcon.Flip = SpriteEffects.None;
            attachedIcon.Scale = new Vector2(1f, 1f);
            m_resourcePool.CheckIn(attachedIcon);
            attachedIcon.AttachedProjectile = null;
            projectile.AttachedIcon = null;
        }

        public void DestroyAllIcons()
        {
            foreach (var current in m_projectileManager.ActiveProjectileList)
                if (current.AttachedIcon != null)
                {
                    DestroyIcon(current);
                }
        }

        public void Update(Camera2D camera)
        {
            var player = m_screenManager.Player;
            foreach (var current in m_projectileManager.ActiveProjectileList)
                if (current.ShowIcon)
                {
                    if (current.AttachedIcon == null)
                    {
                        if (!CollisionMath.Intersects(current.Bounds, camera.Bounds) &&
                            (current.AccelerationX > 1f && current.X < player.X && current.Y > camera.Bounds.Top &&
                             current.Y < camera.Bounds.Bottom ||
                             current.AccelerationX < -1f && current.X > player.X && current.Y > camera.Bounds.Top &&
                             current.Y < camera.Bounds.Bottom ||
                             current.AccelerationY > 1f && current.Y < player.Y && current.X > camera.Bounds.Left &&
                             current.X < camera.Bounds.Right ||
                             current.AccelerationY < -1f && current.Y > player.Y && current.X > camera.Bounds.Left &&
                             current.X < camera.Bounds.Right))
                        {
                            AddIcon(current);
                        }
                    }
                    else if (CollisionMath.Intersects(current.Bounds, camera.Bounds))
                    {
                        DestroyIcon(current);
                    }
                }

            for (var i = 0; i < m_resourcePool.ActiveObjsList.Count; i++)
                if (!m_resourcePool.ActiveObjsList[i].AttachedProjectile.IsAlive)
                {
                    DestroyIcon(m_resourcePool.ActiveObjsList[i].AttachedProjectile);
                    i--;
                }

            foreach (var current2 in m_resourcePool.ActiveObjsList) current2.Update(camera);
        }

        public void Draw(Camera2D camera)
        {
            if (Game.PlayerStats.Traits.X != 21f && Game.PlayerStats.Traits.Y != 21f)
            {
                foreach (var current in m_resourcePool.ActiveObjsList) current.Draw(camera);
            }
        }
    }
}