// 
// RogueLegacyArchipelago - EnemyObj_SpikeTrap.cs
// Last Modified 2021-12-27
// 
// This project is based on the modified disassembly of Rogue Legacy's engine, with permission to do so by its
// original creators. Therefore, former creators' copyright notice applies to the original disassembly.
// 
// Original Disassembled Source - © 2011-2015, Cellar Door Games Inc.
// Rogue Legacy™ is a trademark or registered trademark of Cellar Door Games Inc. All Rights Reserved.
// 

using DS2DEngine;
using Microsoft.Xna.Framework;
using RogueCastle.Structs;

namespace RogueCastle
{
    public class EnemyObj_SpikeTrap : EnemyObj
    {
        private Rectangle DetectionRect;
        private float ExtractDelay;
        private LogicSet m_extractLS;

        public EnemyObj_SpikeTrap(PlayerObj target, PhysicsManager physicsManager,
            ProceduralLevelScreen levelToAttachTo,
            EnemyDifficulty difficulty)
            : base("EnemySpikeTrap_Character", target, physicsManager, levelToAttachTo, difficulty)
        {
            Type = 21;
            StopAnimation();
            PlayAnimationOnRestart = false;
            NonKillable = true;
        }

        private Rectangle AbsDetectionRect
        {
            get
            {
                return new Rectangle((int) (X - DetectionRect.Width / 2f), (int) (Y - DetectionRect.Height),
                    DetectionRect.Width, DetectionRect.Height);
            }
        }

        protected override void InitializeEV()
        {
            Scale = new Vector2(2f, 2f);
            AnimationDelay = 0.1f;
            Speed = 0f;
            MaxHealth = 10;
            EngageRadius = 2100;
            ProjectileRadius = 2200;
            MeleeRadius = 650;
            CooldownTime = 2f;
            KnockBack = new Vector2(1f, 2f);
            Damage = 25;
            JumpHeight = 20.5f;
            AlwaysFaceTarget = false;
            CanFallOffLedges = false;
            XPValue = 2;
            CanBeKnockedBack = false;
            LockFlip = true;
            IsWeighted = false;
            ExtractDelay = 0.1f;
            DetectionRect = new Rectangle(0, 0, 120, 30);
            Name = "Spike Trap";
            /*switch (Difficulty)
            {
                case EnemyDifficulty.BASIC:
                case EnemyDifficulty.ADVANCED:
                case EnemyDifficulty.EXPERT:
                case EnemyDifficulty.MINIBOSS:*/
            //IL_F5:
            IsCollidable = false;
            //return;
            //}
            //goto IL_F5;
        }

        protected override void InitializeLogic()
        {
            m_extractLS = new LogicSet(this);
            m_extractLS.AddAction(new PlayAnimationLogicAction(1, 2));
            m_extractLS.AddAction(new DelayLogicAction(ExtractDelay));
            m_extractLS.AddAction(new Play3DSoundLogicAction(this, Game.ScreenManager.Player, "TrapSpike_01",
                "TrapSpike_02", "TrapSpike_03"));
            m_extractLS.AddAction(new PlayAnimationLogicAction(2, 4));
            base.InitializeLogic();
        }

        protected override void RunBasicLogic()
        {
            switch (State)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    return;
            }
        }

        protected override void RunAdvancedLogic()
        {
            switch (State)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    return;
            }
        }

        protected override void RunExpertLogic()
        {
            switch (State)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    return;
            }
        }

        protected override void RunMinibossLogic()
        {
            switch (State)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    return;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsPaused)
            {
                if (Game.PlayerStats.Traits.X != 23f && Game.PlayerStats.Traits.Y != 23f)
                {
                    if (CollisionMath.Intersects(AbsDetectionRect, m_target.Bounds))
                    {
                        if (CurrentFrame == 1 || CurrentFrame == TotalFrames)
                        {
                            IsCollidable = true;
                            m_extractLS.Execute();
                        }
                    }
                    else if (CurrentFrame == 5 && !m_extractLS.IsActive)
                    {
                        IsCollidable = false;
                        PlayAnimation("StartRetract", "RetractComplete");
                    }
                }

                if (m_extractLS.IsActive)
                {
                    m_extractLS.Update(gameTime);
                }
            }

            base.Update(gameTime);
        }

        public override void Reset()
        {
            PlayAnimation(1, 1);
            base.Reset();
        }

        public override void ResetState()
        {
            PlayAnimation(1, 1);
            base.ResetState();
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                m_extractLS.Dispose();
                m_extractLS = null;
                base.Dispose();
            }
        }
    }
}