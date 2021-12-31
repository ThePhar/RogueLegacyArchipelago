// 
// RogueLegacyArchipelago - EnemyObj.cs
// Last Modified 2021-12-27
// 
// This project is based on the modified disassembly of Rogue Legacy's engine, with permission to do so by its
// original creators. Therefore, former creators' copyright notice applies to the original disassembly.
// 
// Original Disassembled Source - © 2011-2015, Cellar Door Games Inc.
// Rogue Legacy™ is a trademark or registered trademark of Cellar Door Games Inc. All Rights Reserved.
// 

using System;
using System.Collections.Generic;
using System.Globalization;
using DS2DEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueCastle.Structs;
using Tweener;

namespace RogueCastle
{
    public abstract class EnemyObj : CharacterObj, IDealsDamageObj
    {
        protected const int STATE_WANDER = 0;
        protected const int STATE_ENGAGE = 1;
        protected const int STATE_PROJECTILE_ENGAGE = 2;
        protected const int STATE_MELEE_ENGAGE = 3;
        protected bool AlwaysFaceTarget;
        protected bool CanFallOffLedges = true;
        protected float CooldownTime;
        protected int DamageGainPerLevel;
        protected float DistanceToPlayer;
        protected int EngageRadius;
        protected int HealthGainPerLevel;
        public float InitialLogicDelay;
        protected float ItemDropChance;
        protected List<LogicBlock> logicBlocksToDispose;
        protected bool m_bossVersionKilled;
        private LogicBlock m_cooldownLB;
        private int[] m_cooldownParams;
        private float m_cooldownTimer;
        protected LogicBlock m_currentActiveLB;
        protected int m_damage;
        private Texture2D m_engageRadiusTexture;
        protected TweenObject m_flipTween;
        protected float m_initialDelayCounter;
        protected float m_invincibilityTime = 0.4f;
        protected float m_invincibleCounter;
        protected float m_invincibleCounterProjectile;
        private int m_level;
        private Texture2D m_meleeRadiusTexture;
        private int m_numTouchingGrounds;
        private Texture2D m_projectileRadiusTexture;
        protected string m_resetSpriteName;
        private bool m_runCooldown;
        protected bool m_saveToEnemiesKilledList = true;
        protected PlayerObj m_target;
        private LogicBlock m_walkingLB;
        protected int m_xpValue;
        protected int MaxMoneyDropAmount;
        protected float MaxMoneyGainPerLevel;
        protected int MeleeRadius;
        protected int MinMoneyDropAmount;
        protected float MinMoneyGainPerLevel;
        protected float MoneyDropChance;
        protected int ProjectileDamage = 5;
        protected int ProjectileRadius;
        protected float ProjectileSpeed = 100f;
        public bool SaveToFile = true;
        protected float StatLevelDMGMod;
        protected float StatLevelHPMod;
        protected float StatLevelXPMod;
        protected GameObj TintablePart;
        public byte Type;
        protected int XPBonusPerLevel;

        public EnemyObj(string spriteName, PlayerObj target, PhysicsManager physicsManager,
            ProceduralLevelScreen levelToAttachTo, EnemyDifficulty difficulty)
            : base(spriteName, physicsManager, levelToAttachTo)
        {
            DisableCollisionBoxRotations = true;
            Type = 0;
            CollisionTypeTag = 3;
            m_target = target;
            m_walkingLB = new LogicBlock();
            m_currentActiveLB = new LogicBlock();
            m_cooldownLB = new LogicBlock();
            logicBlocksToDispose = new List<LogicBlock>();
            m_resetSpriteName = spriteName;
            Difficulty = difficulty;
            ProjectileScale = new Vector2(1f, 1f);
            PlayAnimation();
            PlayAnimationOnRestart = true;
            OutlineWidth = 2;
            GivesLichHealth = true;
            DropsItem = true;
        }

        public Vector2 ProjectileScale { get; internal set; }
        public bool Procedural { get; set; }
        public bool NonKillable { get; set; }
        public bool GivesLichHealth { get; set; }
        public bool IsDemented { get; set; }

        public int Level
        {
            get { return m_level; }
            set
            {
                m_level = value;
                if (m_level < 0)
                {
                    m_level = 0;
                }
            }
        }

        protected float InvincibilityTime
        {
            get { return m_invincibilityTime; }
        }

        public EnemyDifficulty Difficulty { get; internal set; }
        public bool IsProcedural { get; set; }
        public bool PlayAnimationOnRestart { get; set; }
        public bool DropsItem { get; set; }

        private Rectangle GroundCollisionRect
        {
            get { return new Rectangle(Bounds.X - 10, Bounds.Y, Width + 20, Height + 10); }
        }

        private Rectangle RotatedGroundCollisionRect
        {
            get { return new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height + 40); }
        }

        public override Rectangle Bounds
        {
            get
            {
                if (IsWeighted)
                {
                    return TerrainBounds;
                }

                return base.Bounds;
            }
        }

        public override int MaxHealth
        {
            get { return base.MaxHealth + HealthGainPerLevel * (Level - 1); }
            internal set { base.MaxHealth = value; }
        }

        public int XPValue
        {
            get { return m_xpValue + XPBonusPerLevel * (Level - 1); }
            internal set { m_xpValue = value; }
        }

        public string ResetSpriteName
        {
            get { return m_resetSpriteName; }
        }

        public new bool IsPaused { get; private set; }

        public override SpriteEffects Flip
        {
            get { return base.Flip; }
            set
            {
                if ((Game.PlayerStats.Traits.X == 18f || Game.PlayerStats.Traits.Y == 18f) && Flip != value &&
                    m_levelScreen != null)
                {
                    if (m_flipTween != null && m_flipTween.TweenedObject == this && m_flipTween.Active)
                    {
                        m_flipTween.StopTween(false);
                    }

                    var scaleY = ScaleY;
                    ScaleX = 0f;
                    m_flipTween = Tween.To(this, 0.15f, Tween.EaseNone, "ScaleX", scaleY.ToString());
                }

                base.Flip = value;
            }
        }

        public int Damage
        {
            get { return m_damage + DamageGainPerLevel * (Level - 1); }
            internal set { m_damage = value; }
        }

        private void InitializeBaseEV()
        {
            Speed = 1f;
            MaxHealth = 10;
            EngageRadius = 400;
            ProjectileRadius = 200;
            MeleeRadius = 50;
            KnockBack = Vector2.Zero;
            Damage = 5;
            ProjectileScale = new Vector2(1f, 1f);
            XPValue = 0;
            ProjectileDamage = 5;
            ItemDropChance = 0f;
            MinMoneyDropAmount = 1;
            MaxMoneyDropAmount = 1;
            MoneyDropChance = 0.5f;
            StatLevelHPMod = 0.16f;
            StatLevelDMGMod = 0.091f;
            StatLevelXPMod = 0.025f;
            MinMoneyGainPerLevel = 0.23f;
            MaxMoneyGainPerLevel = 0.29f;
            ForceDraw = true;
        }

        protected override void InitializeEV() { }

        protected override void InitializeLogic()
        {
            var logicSet = new LogicSet(this);
            logicSet.AddAction(new PlayAnimationLogicAction());
            logicSet.AddAction(new MoveLogicAction(m_target, true));
            logicSet.AddAction(new DelayLogicAction(1f));
            var logicSet2 = new LogicSet(this);
            logicSet2.AddAction(new PlayAnimationLogicAction());
            logicSet2.AddAction(new MoveLogicAction(m_target, false));
            logicSet2.AddAction(new DelayLogicAction(1f));
            var logicSet3 = new LogicSet(this);
            logicSet3.AddAction(new StopAnimationLogicAction());
            logicSet3.AddAction(new MoveLogicAction(m_target, true, 0f));
            logicSet3.AddAction(new DelayLogicAction(1f));
            m_walkingLB.AddLogicSet(logicSet, logicSet2, logicSet3);
        }

        public void SetDifficulty(EnemyDifficulty difficulty, bool reinitialize)
        {
            Difficulty = difficulty;
            if (reinitialize)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (TintablePart == null)
            {
                TintablePart = GetChildAt(0);
            }

            InitializeBaseEV();
            InitializeEV();
            HealthGainPerLevel = (int) (base.MaxHealth * StatLevelHPMod);
            DamageGainPerLevel = (int) (m_damage * StatLevelDMGMod);
            XPBonusPerLevel = (int) (m_xpValue * StatLevelXPMod);
            m_internalLockFlip = LockFlip;
            m_internalIsWeighted = IsWeighted;
            m_internalRotation = Rotation;
            m_internalAnimationDelay = AnimationDelay;
            m_internalScale = Scale;
            InternalFlip = Flip;
            foreach (var current in logicBlocksToDispose) current.ClearAllLogicSets();
            if (m_levelScreen != null)
            {
                InitializeLogic();
            }

            m_initialDelayCounter = InitialLogicDelay;
            CurrentHealth = MaxHealth;
        }

        public void InitializeDebugRadii()
        {
            if (m_engageRadiusTexture == null)
            {
                var num = EngageRadius;
                var num2 = ProjectileRadius;
                var num3 = MeleeRadius;
                if (num > 1000)
                {
                    num = 1000;
                }

                if (num2 > 1000)
                {
                    num2 = 1000;
                }

                if (num3 > 1000)
                {
                    num3 = 1000;
                }

                m_engageRadiusTexture = DebugHelper.CreateCircleTexture(num, m_levelScreen.Camera.GraphicsDevice);
                m_projectileRadiusTexture = DebugHelper.CreateCircleTexture(num2, m_levelScreen.Camera.GraphicsDevice);
                m_meleeRadiusTexture = DebugHelper.CreateCircleTexture(num3, m_levelScreen.Camera.GraphicsDevice);
            }
        }

        public void SetPlayerTarget(PlayerObj target)
        {
            m_target = target;
        }

        public void SetLevelScreen(ProceduralLevelScreen levelScreen)
        {
            m_levelScreen = levelScreen;
        }

        public override void Update(GameTime gameTime)
        {
            var num = (float) gameTime.ElapsedGameTime.TotalSeconds;
            if (m_initialDelayCounter > 0f)
            {
                m_initialDelayCounter -= num;
            }
            else
            {
                if (m_invincibleCounter > 0f)
                {
                    m_invincibleCounter -= num;
                }

                if (m_invincibleCounterProjectile > 0f)
                {
                    m_invincibleCounterProjectile -= num;
                }

                if (m_invincibleCounter <= 0f && m_invincibleCounterProjectile <= 0f && !IsWeighted)
                {
                    if (AccelerationY < 0f)
                    {
                        AccelerationY += 15f;
                    }
                    else if (AccelerationY > 0f)
                    {
                        AccelerationY -= 15f;
                    }

                    if (AccelerationX < 0f)
                    {
                        AccelerationX += 15f;
                    }
                    else if (AccelerationX > 0f)
                    {
                        AccelerationX -= 15f;
                    }

                    if (AccelerationY < 3.6f && AccelerationY > -3.6f)
                    {
                        AccelerationY = 0f;
                    }

                    if (AccelerationX < 3.6f && AccelerationX > -3.6f)
                    {
                        AccelerationX = 0f;
                    }
                }

                if (!IsKilled && !IsPaused)
                {
                    DistanceToPlayer = CDGMath.DistanceBetweenPts(Position, m_target.Position);
                    if (DistanceToPlayer > EngageRadius)
                    {
                        State = 0;
                    }
                    else if (DistanceToPlayer < EngageRadius && DistanceToPlayer >= ProjectileRadius)
                    {
                        State = 1;
                    }
                    else if (DistanceToPlayer < ProjectileRadius && DistanceToPlayer >= MeleeRadius)
                    {
                        State = 2;
                    }
                    else
                    {
                        State = 3;
                    }

                    if (m_cooldownTimer > 0f && m_currentActiveLB == m_cooldownLB)
                    {
                        m_cooldownTimer -= (float) gameTime.ElapsedGameTime.TotalSeconds;
                    }

                    if (m_cooldownTimer <= 0f && m_runCooldown)
                    {
                        m_runCooldown = false;
                    }

                    if (!LockFlip)
                    {
                        if (!AlwaysFaceTarget)
                        {
                            if (Heading.X < 0f)
                            {
                                Flip = SpriteEffects.FlipHorizontally;
                            }
                            else
                            {
                                Flip = SpriteEffects.None;
                            }
                        }
                        else if (X > m_target.X)
                        {
                            Flip = SpriteEffects.FlipHorizontally;
                        }
                        else
                        {
                            Flip = SpriteEffects.None;
                        }
                    }

                    if (!m_currentActiveLB.IsActive && !m_runCooldown)
                    {
                        switch (Difficulty)
                        {
                            case EnemyDifficulty.Basic:
                                RunBasicLogic();
                                break;

                            case EnemyDifficulty.Advanced:
                                RunAdvancedLogic();
                                break;

                            case EnemyDifficulty.Expert:
                                RunExpertLogic();
                                break;

                            case EnemyDifficulty.MiniBoss:
                                RunMinibossLogic();
                                break;
                        }

                        if (m_runCooldown && m_currentActiveLB.ActiveLS.Tag == 2)
                        {
                            m_cooldownTimer = CooldownTime;
                        }
                    }

                    if (!m_currentActiveLB.IsActive && m_runCooldown && m_cooldownTimer > 0f && !m_cooldownLB.IsActive)
                    {
                        m_currentActiveLB = m_cooldownLB;
                        m_currentActiveLB.RunLogicBlock(m_cooldownParams);
                    }

                    if (IsWeighted && m_invincibleCounter <= 0f && m_invincibleCounterProjectile <= 0f)
                    {
                        if (HeadingX > 0f)
                        {
                            HeadingX = 1f;
                        }
                        else if (HeadingX < 0f)
                        {
                            HeadingX = -1f;
                        }

                        X += HeadingX * (CurrentSpeed * num);
                    }
                    else if (m_isTouchingGround || !IsWeighted)
                    {
                        Position += Heading * (CurrentSpeed * num);
                    }

                    if (X < m_levelScreen.CurrentRoom.Bounds.Left)
                    {
                        X = m_levelScreen.CurrentRoom.Bounds.Left;
                    }
                    else if (X > m_levelScreen.CurrentRoom.Bounds.Right)
                    {
                        X = m_levelScreen.CurrentRoom.Bounds.Right;
                    }

                    if (Y < m_levelScreen.CurrentRoom.Bounds.Top)
                    {
                        Y = m_levelScreen.CurrentRoom.Bounds.Top;
                    }
                    else if (Y > m_levelScreen.CurrentRoom.Bounds.Bottom)
                    {
                        Y = m_levelScreen.CurrentRoom.Bounds.Bottom;
                    }

                    if (m_currentActiveLB == m_cooldownLB)
                    {
                        m_currentActiveLB.Update(gameTime);
                    }
                    else
                    {
                        m_currentActiveLB.Update(gameTime);
                        m_cooldownLB.Update(gameTime);
                    }
                }
            }

            if (IsWeighted)
            {
                CheckGroundCollision();
            }

            if (CurrentHealth <= 0 && !IsKilled && !m_bossVersionKilled)
            {
                Kill();
            }

            base.Update(gameTime);
        }

        public void CheckGroundCollisionOld()
        {
            m_numTouchingGrounds = 0;
            var num = 2.14748365E+09f;
            var num2 = 10;
            var flag = true;
            foreach (var current in m_levelScreen.PhysicsManager.ObjectList)
                if (current != this && current.CollidesTop &&
                    (current.CollisionTypeTag == 1 || current.CollisionTypeTag == 5 || current.CollisionTypeTag == 4 ||
                     current.CollisionTypeTag == 10) && Math.Abs(current.Bounds.Top - Bounds.Bottom) < num2)
                {
                    foreach (var current2 in current.CollisionBoxes)
                        if (current2.Type == 0)
                        {
                            var a = GroundCollisionRect;
                            if (current2.AbsRotation != 0f)
                            {
                                a = RotatedGroundCollisionRect;
                            }

                            if (CollisionMath.RotatedRectIntersects(a, 0f, Vector2.Zero, current2.AbsRect,
                                    current2.AbsRotation, Vector2.Zero))
                            {
                                m_numTouchingGrounds++;
                                if (current2.AbsParent.Rotation == 0f)
                                {
                                    flag = false;
                                }

                                var vector = CollisionMath.RotatedRectIntersectsMTD(GroundCollisionRect, 0f,
                                    Vector2.Zero, current2.AbsRect, current2.AbsRotation, Vector2.Zero);
                                if (flag)
                                {
                                    flag =
                                        !CollisionMath.RotatedRectIntersects(Bounds, 0f, Vector2.Zero, current2.AbsRect,
                                            current2.AbsRotation, Vector2.Zero);
                                }

                                var y = vector.Y;
                                if (num > y)
                                {
                                    num = y;
                                }
                            }
                        }
                }

            if (num <= 2f && AccelerationY >= 0f)
            {
                m_isTouchingGround = true;
            }
        }

        private void CheckGroundCollision()
        {
            m_isTouchingGround = false;
            m_numTouchingGrounds = 0;
            if (AccelerationY >= 0f)
            {
                IPhysicsObj physicsObj = null;
                var num = 3.40282347E+38f;
                IPhysicsObj physicsObj2 = null;
                var num2 = 3.40282347E+38f;
                var terrainBounds = TerrainBounds;
                terrainBounds.Height += 10;
                foreach (var current in m_levelScreen.CurrentRoom.TerrainObjList)
                {
                    if (current.Visible && current.IsCollidable && current.CollidesTop && current.HasTerrainHitBox &&
                        (current.CollisionTypeTag == 1 || current.CollisionTypeTag == 10 ||
                         current.CollisionTypeTag == 6 || current.CollisionTypeTag == 4))
                    {
                        if (current.Rotation == 0f)
                        {
                            var left = terrainBounds;
                            left.X -= 30;
                            left.Width += 60;
                            var value = CollisionMath.CalculateMTD(left, current.Bounds);
                            if (value != Vector2.Zero)
                            {
                                m_numTouchingGrounds++;
                            }

                            if (CollisionMath.CalculateMTD(terrainBounds, current.Bounds).Y < 0f)
                            {
                                var num3 = current.Bounds.Top - Bounds.Bottom;
                                if (num3 < num)
                                {
                                    physicsObj = current;
                                    num = num3;
                                }
                            }
                        }
                        else
                        {
                            var value2 = CollisionMath.RotatedRectIntersectsMTD(terrainBounds, Rotation,
                                Vector2.Zero, current.TerrainBounds, current.Rotation, Vector2.Zero);
                            if (value2 != Vector2.Zero)
                            {
                                m_numTouchingGrounds++;
                            }

                            if (value2.Y < 0f)
                            {
                                var y = value2.Y;
                                if (y < num2 && value2.Y < 0f)
                                {
                                    physicsObj2 = current;
                                    num2 = y;
                                }
                            }

                            var terrainBounds2 = TerrainBounds;
                            terrainBounds2.Height += 50;
                            var num4 = 15;
                            var pt = CollisionMath.RotatedRectIntersectsMTD(terrainBounds2, Rotation, Vector2.Zero,
                                current.TerrainBounds, current.Rotation, Vector2.Zero);
                            if (pt.Y < 0f)
                            {
                                var num5 = CDGMath.DistanceBetweenPts(pt, Vector2.Zero);
                                var num6 = (float) (50.0 - Math.Sqrt(num5 * num5 * 2f));
                                if (num6 > 0f && num6 < num4)
                                {
                                    Y += num6;
                                }

                                var y2 = value2.Y;
                                if (y2 < num2)
                                {
                                    physicsObj2 = current;
                                    num2 = y2;
                                }
                            }
                        }
                    }

                    if (physicsObj != null)
                    {
                        m_isTouchingGround = true;
                    }

                    if (physicsObj2 != null)
                    {
                        m_isTouchingGround = true;
                    }
                }
            }
        }

        private void HookToSlope(IPhysicsObj collisionObj)
        {
            UpdateCollisionBoxes();
            var terrainBounds = TerrainBounds;
            terrainBounds.Height += 100;
            var num = X;
            if (
                CollisionMath.RotatedRectIntersectsMTD(terrainBounds, Rotation, Vector2.Zero,
                    collisionObj.TerrainBounds,
                    collisionObj.Rotation, Vector2.Zero).Y < 0f)
            {
                var flag = false;
                Vector2 vector;
                Vector2 vector2;
                if (collisionObj.Width > collisionObj.Height)
                {
                    vector = CollisionMath.UpperLeftCorner(collisionObj.TerrainBounds, collisionObj.Rotation,
                        Vector2.Zero);
                    vector2 = CollisionMath.UpperRightCorner(collisionObj.TerrainBounds, collisionObj.Rotation,
                        Vector2.Zero);
                    if (collisionObj.Rotation > 0f)
                    {
                        num = TerrainBounds.Left;
                    }
                    else
                    {
                        num = TerrainBounds.Right;
                    }

                    if (num > vector.X && num < vector2.X)
                    {
                        flag = true;
                    }
                }
                else if (collisionObj.Rotation > 0f)
                {
                    vector = CollisionMath.LowerLeftCorner(collisionObj.TerrainBounds, collisionObj.Rotation,
                        Vector2.Zero);
                    vector2 = CollisionMath.UpperLeftCorner(collisionObj.TerrainBounds, collisionObj.Rotation,
                        Vector2.Zero);
                    num = TerrainBounds.Right;
                    if (num > vector.X && num < vector2.X)
                    {
                        flag = true;
                    }
                }
                else
                {
                    vector = CollisionMath.UpperRightCorner(collisionObj.TerrainBounds, collisionObj.Rotation,
                        Vector2.Zero);
                    vector2 = CollisionMath.LowerRightCorner(collisionObj.TerrainBounds, collisionObj.Rotation,
                        Vector2.Zero);
                    num = TerrainBounds.Left;
                    if (num > vector.X && num < vector2.X)
                    {
                        flag = true;
                    }
                }

                if (flag)
                {
                    var num2 = vector2.X - vector.X;
                    var num3 = vector2.Y - vector.Y;
                    var x = vector.X;
                    var y = vector.Y;
                    var num4 = y + (num - x) * (num3 / num2);
                    num4 -= TerrainBounds.Bottom - Y - 2f;
                    Y = (int) num4;
                }
            }
        }

        protected void SetCooldownLogicBlock(LogicBlock cooldownLB, params int[] percentage)
        {
            m_cooldownLB = cooldownLB;
            m_cooldownParams = percentage;
        }

        protected void RunLogicBlock(bool runCDLogicAfterward, LogicBlock block, params int[] percentage)
        {
            m_runCooldown = runCDLogicAfterward;
            m_currentActiveLB = block;
            m_currentActiveLB.RunLogicBlock(percentage);
        }

        protected virtual void RunBasicLogic() { }

        protected virtual void RunAdvancedLogic()
        {
            RunBasicLogic();
        }

        protected virtual void RunExpertLogic()
        {
            RunBasicLogic();
        }

        protected virtual void RunMinibossLogic()
        {
            RunBasicLogic();
        }

        public override void CollisionResponse(CollisionBox thisBox, CollisionBox otherBox, int collisionResponseType)
        {
            var physicsObj = otherBox.AbsParent as IPhysicsObj;
            var vector = CollisionMath.CalculateMTD(thisBox.AbsRect, otherBox.AbsRect);
            if (collisionResponseType == 2 &&
                (physicsObj.CollisionTypeTag == 2 || physicsObj.CollisionTypeTag == 10 ||
                 physicsObj.CollisionTypeTag == 10 && IsWeighted) &&
                (!(otherBox.AbsParent is ProjectileObj) && m_invincibleCounter <= 0f ||
                 otherBox.AbsParent is ProjectileObj &&
                 (m_invincibleCounterProjectile <= 0f ||
                  (otherBox.AbsParent as ProjectileObj).IgnoreInvincibleCounter)))
            {
                if (IsDemented)
                {
                    m_invincibleCounter = InvincibilityTime;
                    m_invincibleCounterProjectile = InvincibilityTime;
                    m_levelScreen.ImpactEffectPool.DisplayQuestionMark(new Vector2(X, Bounds.Top));
                    return;
                }

                var num = (physicsObj as IDealsDamageObj).Damage;
                var isPlayer = false;
                if (physicsObj == m_target)
                {
                    if (CDGMath.RandomFloat(0f, 1f) <= m_target.TotalCritChance && !NonKillable &&
                        physicsObj == m_target)
                    {
                        m_levelScreen.ImpactEffectPool.DisplayCriticalText(new Vector2(X, Bounds.Top));
                        num = (int) (num * m_target.TotalCriticalDamage);
                    }

                    isPlayer = true;
                }

                var projectileObj = otherBox.AbsParent as ProjectileObj;
                if (projectileObj != null)
                {
                    m_invincibleCounterProjectile = InvincibilityTime;
                    if (projectileObj.DestroysWithEnemy && !NonKillable)
                    {
                        projectileObj.RunDestroyAnimation(false);
                    }
                }

                var center = Rectangle.Intersect(thisBox.AbsRect, otherBox.AbsRect).Center;
                if (thisBox.AbsRotation != 0f || otherBox.AbsRotation != 0f)
                {
                    center = Rectangle.Intersect(thisBox.AbsParent.Bounds, otherBox.AbsParent.Bounds).Center;
                }

                var collisionPt = new Vector2(center.X, center.Y);
                if (projectileObj == null || projectileObj != null && projectileObj.Spell != 20)
                {
                    if (projectileObj != null || physicsObj.CollisionTypeTag != 10 ||
                        physicsObj.CollisionTypeTag == 10 && IsWeighted)
                    {
                        HitEnemy(num, collisionPt, isPlayer);
                    }
                }
                else if (projectileObj != null && projectileObj.Spell == 20 && CanBeKnockedBack && !IsPaused)
                {
                    CurrentSpeed = 0f;
                    var num2 = 3f;
                    if (KnockBack == Vector2.Zero)
                    {
                        if (X < m_target.X)
                        {
                            AccelerationX = -m_target.EnemyKnockBack.X * num2;
                        }
                        else
                        {
                            AccelerationX = m_target.EnemyKnockBack.X * num2;
                        }

                        AccelerationY = -m_target.EnemyKnockBack.Y * num2;
                    }
                    else
                    {
                        if (X < m_target.X)
                        {
                            AccelerationX = -KnockBack.X * num2;
                        }
                        else
                        {
                            AccelerationX = KnockBack.X * num2;
                        }

                        AccelerationY = -KnockBack.Y * num2;
                    }
                }

                if (physicsObj == m_target)
                {
                    m_invincibleCounter = InvincibilityTime;
                }
            }

            if (collisionResponseType == 1 &&
                (physicsObj.CollisionTypeTag == 1 || physicsObj.CollisionTypeTag == 6 ||
                 physicsObj.CollisionTypeTag == 10) && CollisionTypeTag != 4)
            {
                if (CurrentSpeed != 0f && vector.X != 0f && Math.Abs(vector.X) > 10f &&
                    (vector.X > 0f && physicsObj.CollidesRight || vector.X < 0f && physicsObj.CollidesLeft))
                {
                    CurrentSpeed = 0f;
                }

                if (m_numTouchingGrounds <= 1 && CurrentSpeed != 0f && vector.Y < 0f && !CanFallOffLedges)
                {
                    if (Bounds.Left < physicsObj.Bounds.Left && HeadingX < 0f)
                    {
                        X = physicsObj.Bounds.Left + (AbsX - Bounds.Left);
                        CurrentSpeed = 0f;
                    }
                    else if (Bounds.Right > physicsObj.Bounds.Right && HeadingX > 0f)
                    {
                        X = physicsObj.Bounds.Right - (Bounds.Right - AbsX);
                        CurrentSpeed = 0f;
                    }

                    m_isTouchingGround = true;
                }

                if (AccelerationX != 0f && m_isTouchingGround)
                {
                    AccelerationX = 0f;
                }

                var flag = false;
                if (Math.Abs(vector.X) < 10f && vector.X != 0f && Math.Abs(vector.Y) < 10f && vector.Y != 0f)
                {
                    flag = true;
                }

                if (m_isTouchingGround && !physicsObj.CollidesBottom && physicsObj.CollidesTop &&
                    physicsObj.TerrainBounds.Top < TerrainBounds.Bottom - 30)
                {
                    flag = true;
                }

                if (!physicsObj.CollidesRight && !physicsObj.CollidesLeft && physicsObj.CollidesTop &&
                    physicsObj.CollidesBottom)
                {
                    flag = true;
                }

                var vector2 = CollisionMath.RotatedRectIntersectsMTD(thisBox.AbsRect, thisBox.AbsRotation,
                    Vector2.Zero, otherBox.AbsRect, otherBox.AbsRotation, Vector2.Zero);
                if (!flag)
                {
                    base.CollisionResponse(thisBox, otherBox, collisionResponseType);
                }

                if (vector2.Y < 0f && otherBox.AbsRotation != 0f && IsWeighted)
                {
                    X -= vector2.X;
                }
            }
        }

        public virtual void HitEnemy(int damage, Vector2 collisionPt, bool isPlayer)
        {
            if (m_target != null && m_target.CurrentHealth > 0)
            {
                SoundManager.Play3DSound(this, Game.ScreenManager.Player, "EnemyHit1", "EnemyHit2", "EnemyHit3",
                    "EnemyHit4", "EnemyHit5", "EnemyHit6");
                Blink(Color.Red, 0.1f);
                m_levelScreen.ImpactEffectPool.DisplayEnemyImpactEffect(collisionPt);
                if (isPlayer && (Game.PlayerStats.Class == 6 || Game.PlayerStats.Class == 14))
                {
                    CurrentHealth -= damage;
                    m_target.CurrentMana += (int) (damage * 0.3f);
                    m_levelScreen.TextManager.DisplayNumberText(damage, Color.Red, new Vector2(X, Bounds.Top));
                    m_levelScreen.TextManager.DisplayNumberStringText((int) (damage * 0.3f), "mp", Color.RoyalBlue,
                        new Vector2(m_target.X, m_target.Bounds.Top - 30));
                }
                else
                {
                    CurrentHealth -= damage;
                    m_levelScreen.TextManager.DisplayNumberText(damage, Color.Red, new Vector2(X, Bounds.Top));
                }

                if (isPlayer)
                {
                    var expr_198 = m_target;
                    expr_198.NumSequentialAttacks += 1;
                    if (m_target.IsAirAttacking)
                    {
                        m_target.IsAirAttacking = false;
                        m_target.AccelerationY = -m_target.AirAttackKnockBack;
                        m_target.NumAirBounces++;
                    }
                }

                if (CanBeKnockedBack && !IsPaused && Game.PlayerStats.Traits.X != 17f &&
                    Game.PlayerStats.Traits.Y != 17f)
                {
                    CurrentSpeed = 0f;
                    var num = 1f;
                    if (Game.PlayerStats.Traits.X == 16f || Game.PlayerStats.Traits.Y == 16f)
                    {
                        num = 2f;
                    }

                    if (KnockBack == Vector2.Zero)
                    {
                        if (X < m_target.X)
                        {
                            AccelerationX = -m_target.EnemyKnockBack.X * num;
                        }
                        else
                        {
                            AccelerationX = m_target.EnemyKnockBack.X * num;
                        }

                        AccelerationY = -m_target.EnemyKnockBack.Y * num;
                    }
                    else
                    {
                        if (X < m_target.X)
                        {
                            AccelerationX = -KnockBack.X * num;
                        }
                        else
                        {
                            AccelerationX = KnockBack.X * num;
                        }

                        AccelerationY = -KnockBack.Y * num;
                    }
                }

                m_levelScreen.SetLastEnemyHit(this);
            }
        }

        public void KillSilently()
        {
            base.Kill(false);
        }

        public override void Kill(bool giveXP = true)
        {
            var totalVampBonus = m_target.TotalVampBonus;
            if (totalVampBonus > 0)
            {
                m_target.CurrentHealth += totalVampBonus;
                m_levelScreen.TextManager.DisplayNumberStringText(totalVampBonus, "hp", Color.LightGreen,
                    new Vector2(m_target.X, m_target.Bounds.Top - 60));
            }

            if (m_target.ManaGain > 0f)
            {
                m_target.CurrentMana += m_target.ManaGain;
                m_levelScreen.TextManager.DisplayNumberStringText((int) m_target.ManaGain, "mp", Color.RoyalBlue,
                    new Vector2(m_target.X, m_target.Bounds.Top - 90));
            }

            if (Game.PlayerStats.SpecialItem == 5)
            {
                m_levelScreen.ItemDropManager.DropItem(Position, 1, 10f);
                m_levelScreen.ItemDropManager.DropItem(Position, 1, 10f);
            }

            m_levelScreen.KillEnemy(this);
            SoundManager.Play3DSound(this, Game.ScreenManager.Player, "Enemy_Death");
            if (DropsItem)
            {
                if (Type == 26)
                {
                    m_levelScreen.ItemDropManager.DropItem(Position, 2, 0.1f);
                }
                else if (CDGMath.RandomInt(1, 100) <= 2)
                {
                    if (CDGMath.RandomPlusMinus() < 0)
                    {
                        m_levelScreen.ItemDropManager.DropItem(Position, 2, 0.1f);
                    }
                    else
                    {
                        m_levelScreen.ItemDropManager.DropItem(Position, 3, 0.1f);
                    }
                }

                if (CDGMath.RandomFloat(0f, 1f) <= MoneyDropChance)
                {
                    var num = CDGMath.RandomInt(MinMoneyDropAmount, MaxMoneyDropAmount) * 10 +
                              (int) (CDGMath.RandomFloat(MinMoneyGainPerLevel, MaxMoneyGainPerLevel) * Level * 10f);
                    var num2 = num / 500;
                    num -= num2 * 500;
                    var num3 = num / 100;
                    num -= num3 * 100;
                    var num4 = num / 10;
                    for (var i = 0; i < num2; i++) m_levelScreen.ItemDropManager.DropItem(Position, 11, 500f);
                    for (var j = 0; j < num3; j++) m_levelScreen.ItemDropManager.DropItem(Position, 10, 100f);
                    for (var k = 0; k < num4; k++) m_levelScreen.ItemDropManager.DropItem(Position, 1, 10f);
                }
            }

            if (m_currentActiveLB.IsActive)
            {
                m_currentActiveLB.StopLogicBlock();
            }

            m_levelScreen.ImpactEffectPool.DisplayDeathEffect(Position);
            if ((Game.PlayerStats.Class == 7 || Game.PlayerStats.Class == 15) && GivesLichHealth)
            {
                var num5 = 0;
                var currentLevel = Game.PlayerStats.CurrentLevel;
                var num6 = (int) (Level * 2.75f);
                if (currentLevel < num6)
                {
                    num5 = 4;
                }
                else if (currentLevel >= num6)
                {
                    num5 = 4;
                }

                var num7 =
                    (int)
                    Math.Round(
                        (m_target.BaseHealth + m_target.GetEquipmentHealth() + Game.PlayerStats.BonusHealth * 5 +
                         SkillSystem.GetSkill(SkillType.HealthUp).ModifierAmount +
                         SkillSystem.GetSkill(SkillType.HealthUpFinal).ModifierAmount) * 1f,
                        MidpointRounding.AwayFromZero);
                if (m_target.MaxHealth + num5 < num7)
                {
                    Game.PlayerStats.LichHealth += num5;
                    m_target.CurrentHealth += num5;
                    m_levelScreen.TextManager.DisplayNumberStringText(num5, "max hp", Color.LightGreen,
                        new Vector2(m_target.X, m_target.Bounds.Top - 30));
                }
            }

            Game.PlayerStats.NumEnemiesBeaten++;
            if (m_saveToEnemiesKilledList)
            {
                var value = Game.PlayerStats.EnemiesKilledList[Type];
                switch (Difficulty)
                {
                    case EnemyDifficulty.Basic:
                        value.X += 1f;
                        break;

                    case EnemyDifficulty.Advanced:
                        value.Y += 1f;
                        break;

                    case EnemyDifficulty.Expert:
                        value.Z += 1f;
                        break;

                    case EnemyDifficulty.MiniBoss:
                        value.W += 1f;
                        break;
                }

                Game.PlayerStats.EnemiesKilledList[Type] = value;
            }

            if (giveXP && Type == 26)
            {
                GameUtil.UnlockAchievement("FEAR_OF_CHICKENS");
            }

            base.Kill();
        }

        public void PauseEnemy(bool forcePause = false)
        {
            if (!IsPaused && !IsKilled && !m_bossVersionKilled || forcePause)
            {
                IsPaused = true;
                DisableAllWeight = true;
                PauseAnimation();
            }
        }

        public void UnpauseEnemy(bool forceUnpause = false)
        {
            if (IsPaused && !IsKilled && !m_bossVersionKilled || forceUnpause)
            {
                IsPaused = false;
                DisableAllWeight = false;
                ResumeAnimation();
            }
        }

        public void DrawDetectionRadii(Camera2D camera)
        {
            camera.Draw(m_engageRadiusTexture, new Vector2(Position.X - EngageRadius, Position.Y - EngageRadius),
                Color.Red * 0.5f);
            camera.Draw(m_projectileRadiusTexture,
                new Vector2(Position.X - ProjectileRadius, Position.Y - ProjectileRadius), Color.Blue * 0.5f);
            camera.Draw(m_meleeRadiusTexture, new Vector2(Position.X - MeleeRadius, Position.Y - MeleeRadius),
                Color.Green * 0.5f);
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                if (m_currentActiveLB.IsActive)
                {
                    m_currentActiveLB.StopLogicBlock();
                }

                m_currentActiveLB = null;
                foreach (var current in logicBlocksToDispose) current.Dispose();
                for (var i = 0; i < logicBlocksToDispose.Count; i++) logicBlocksToDispose[i] = null;
                logicBlocksToDispose.Clear();
                logicBlocksToDispose = null;
                m_target = null;
                m_walkingLB.Dispose();
                m_walkingLB = null;
                if (m_cooldownLB.IsActive)
                {
                    m_cooldownLB.StopLogicBlock();
                }

                m_cooldownLB.Dispose();
                m_cooldownLB = null;
                if (m_engageRadiusTexture != null)
                {
                    m_engageRadiusTexture.Dispose();
                }

                m_engageRadiusTexture = null;
                if (m_engageRadiusTexture != null)
                {
                    m_projectileRadiusTexture.Dispose();
                }

                m_projectileRadiusTexture = null;
                if (m_engageRadiusTexture != null)
                {
                    m_meleeRadiusTexture.Dispose();
                }

                m_meleeRadiusTexture = null;
                if (m_cooldownParams != null)
                {
                    Array.Clear(m_cooldownParams, 0, m_cooldownParams.Length);
                }

                m_cooldownParams = null;
                TintablePart = null;
                m_flipTween = null;
                base.Dispose();
            }
        }

        public override void Reset()
        {
            if (m_currentActiveLB.IsActive)
            {
                m_currentActiveLB.StopLogicBlock();
            }

            if (m_cooldownLB.IsActive)
            {
                m_cooldownLB.StopLogicBlock();
            }

            m_invincibleCounter = 0f;
            m_invincibleCounterProjectile = 0f;
            State = 0;
            ChangeSprite(m_resetSpriteName);
            if (PlayAnimationOnRestart)
            {
                PlayAnimation();
            }

            m_initialDelayCounter = InitialLogicDelay;
            UnpauseEnemy(true);
            m_bossVersionKilled = false;
            m_blinkTimer = 0f;
            base.Reset();
        }

        public virtual void ResetState()
        {
            if (m_currentActiveLB.IsActive)
            {
                m_currentActiveLB.StopLogicBlock();
            }

            if (m_cooldownLB.IsActive)
            {
                m_cooldownLB.StopLogicBlock();
            }

            m_invincibleCounter = 0f;
            m_invincibleCounterProjectile = 0f;
            State = 0;
            if (Type != 32)
            {
                ChangeSprite(m_resetSpriteName);
            }

            if (PlayAnimationOnRestart)
            {
                PlayAnimation();
            }

            m_initialDelayCounter = InitialLogicDelay;
            LockFlip = m_internalLockFlip;
            Flip = InternalFlip;
            AnimationDelay = m_internalAnimationDelay;
            UnpauseEnemy(true);
            CurrentHealth = MaxHealth;
            m_blinkTimer = 0f;
        }

        protected float ParseTagToFloat(string key)
        {
            if (Tag != "")
            {
                var num = Tag.IndexOf(key + ":") + key.Length + 1;
                var num2 = Tag.IndexOf(",", num);
                if (num2 == -1)
                {
                    num2 = Tag.Length;
                }

                try
                {
                    var cultureInfo = (CultureInfo) CultureInfo.CurrentCulture.Clone();
                    cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
                    var result = float.Parse(Tag.Substring(num, num2 - num), NumberStyles.Any, cultureInfo);
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Concat("Could not parse key:", key, " with string:", Tag,
                        ".  Original Error: ", ex.Message));
                    var result = 0f;
                    return result;
                }
            }

            return 0f;
        }

        protected string ParseTagToString(string key)
        {
            var num = Tag.IndexOf(key + ":") + key.Length + 1;
            var num2 = Tag.IndexOf(",", num);
            if (num2 == -1)
            {
                num2 = Tag.Length;
            }

            return Tag.Substring(num, num2 - num);
        }

        protected override GameObj CreateCloneInstance()
        {
            return EnemyBuilder.BuildEnemy(Type, m_target, null, m_levelScreen, Difficulty);
        }

        protected override void FillCloneInstance(object obj)
        {
            base.FillCloneInstance(obj);
            var enemyObj = obj as EnemyObj;
            enemyObj.IsProcedural = IsProcedural;
            enemyObj.InitialLogicDelay = InitialLogicDelay;
            enemyObj.NonKillable = NonKillable;
            enemyObj.GivesLichHealth = GivesLichHealth;
            enemyObj.DropsItem = DropsItem;
            enemyObj.IsDemented = IsDemented;
        }
    }
}