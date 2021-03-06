﻿namespace Flowers_ADC_Series.Common
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using Prediction;
    using SharpDX;
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public static class Common
    {
        public static bool SebbyLibIsSpellHeroCollision(Obj_AI_Hero t, Spell QWER, int extraWith = 50)
        {
            foreach (
                var hero in
                HeroManager.Enemies.FindAll(
                    hero =>
                        hero.IsValidTarget(QWER.Range + QWER.Width, true, QWER.RangeCheckFrom) &&
                        t.NetworkId != hero.NetworkId))
            {
                var prediction = QWER.GetPrediction(hero);
                var powCalc = Math.Pow(QWER.Width + extraWith + hero.BoundingRadius, 2);

                if (prediction.UnitPosition.To2D()
                        .Distance(QWER.From.To2D(), QWER.GetPrediction(t).CastPosition.To2D(), true, true) <= powCalc)
                {
                    return true;
                }

                if (prediction.UnitPosition.To2D().Distance(QWER.From.To2D(), t.ServerPosition.To2D(), true, true) <=
                    powCalc)
                {
                    return true;
                }

            }
            return false;
        }

        public static bool SebbyLibIsMovingInSameDirection(Obj_AI_Base source, Obj_AI_Base target)
        {
            var sourceLW = source.GetWaypoints().Last().To3D();

            if (sourceLW == source.Position || !source.IsMoving)
            {
                return false;
            }

            var targetLW = target.GetWaypoints().Last().To3D();

            if (targetLW == target.Position || !target.IsMoving)
            {
                return false;
            }

            var pos1 = sourceLW.To2D() - source.Position.To2D();
            var pos2 = targetLW.To2D() - target.Position.To2D();
            var getAngle = pos1.AngleBetween(pos2);

            return getAngle < 25;
        }

        public static List<Vector3> SebbyLibCirclePoints(float CircleLineSegmentN, float radius, Vector3 position)
        {
            var points = new List<Vector3>();

            for (var i = 1; i <= CircleLineSegmentN; i++)
            {
                var angle = i * 2 * Math.PI / CircleLineSegmentN;
                var point = new Vector3(position.X + radius*(float) Math.Cos(angle),
                    position.Y + radius*(float) Math.Sin(angle), position.Z);

                points.Add(point);
            }

            return points;
        }

        private static HitChance MinCommonHitChance
        {
            get
            {
                if (Logic.spellHitChance == 1)
                {
                    return HitChance.High;
                }

                if (Logic.spellHitChance == 2)
                {
                    return HitChance.Medium;
                }

                if (Logic.spellHitChance == 3)
                {
                    return HitChance.Low;
                }

                return HitChance.VeryHigh;
            }
        }

        private static OktwPrediction.HitChance MinOKTWHitChance
        {
            get
            {
                if (Logic.spellHitChance == 1)
                {
                    return OktwPrediction.HitChance.High;
                }

                if (Logic.spellHitChance == 2)
                {
                    return OktwPrediction.HitChance.Medium;
                }

                if (Logic.spellHitChance == 3)
                {
                    return OktwPrediction.HitChance.Low;
                }

                return OktwPrediction.HitChance.VeryHigh;
            }
        }

        private static SDKPrediction.HitChance MinSDKHitChance
        {
            get
            {
                if (Logic.spellHitChance == 1)
                {
                    return SDKPrediction.HitChance.High;
                }

                if (Logic.spellHitChance == 2)
                {
                    return SDKPrediction.HitChance.Medium;
                }

                if (Logic.spellHitChance == 3)
                {
                    return SDKPrediction.HitChance.Low;
                }

                return SDKPrediction.HitChance.VeryHigh;
            }
        }

        public static void CastTo(this Spell Spells, Obj_AI_Base target, bool AOE = false)
        {
            switch (Logic.SelectPred)
            {
                case 0:
                    {
                        var SpellPred = Spells.GetPrediction(target, AOE);

                        if (SpellPred.Hitchance >= MinCommonHitChance)
                        {
                            Spells.Cast(SpellPred.CastPosition, true);
                        }
                    }
                    break;
                case 1:
                    {
                        var CoreType2 = OktwPrediction.SkillshotType.SkillshotLine;
                        var aoe2 = false;

                        if (Spells.Type == SkillshotType.SkillshotCircle)
                        {
                            CoreType2 = OktwPrediction.SkillshotType.SkillshotCircle;
                            aoe2 = true;
                        }

                        if (Spells.Width > 80 && !Spells.Collision)
                            aoe2 = true;

                        var predInput2 = new OktwPrediction.PredictionInput
                        {
                            Aoe = aoe2,
                            Collision = Spells.Collision,
                            Speed = Spells.Speed,
                            Delay = Spells.Delay,
                            Range = Spells.Range,
                            From = ObjectManager.Player.ServerPosition,
                            Radius = Spells.Width,
                            Unit = target,
                            Type = CoreType2
                        };
                        var poutput2 = OktwPrediction.Prediction.GetPrediction(predInput2);

                        if (Spells.Speed != float.MaxValue &&
                            YasuoWindWall.CollisionYasuo(ObjectManager.Player.ServerPosition, poutput2.CastPosition))
                        {
                            return;
                        }

                        if (poutput2.Hitchance >= MinOKTWHitChance)
                        {
                            Spells.Cast(poutput2.CastPosition, true);
                        }

                        if (predInput2.Aoe && poutput2.AoeTargetsHitCount > 1 && poutput2.Hitchance >= MinOKTWHitChance - 1)
                        {
                            Spells.Cast(poutput2.CastPosition, true);
                        }
                    }
                    break;
                case 2:
                    {
                        var CoreType2 = SDKPrediction.SkillshotType.SkillshotLine;

                        var predInput2 = new SDKPrediction.PredictionInput
                        {
                            AoE = AOE,
                            Collision = Spells.Collision,
                            Speed = Spells.Speed,
                            Delay = Spells.Delay,
                            Range = Spells.Range,
                            From = ObjectManager.Player.ServerPosition,
                            Radius = Spells.Width,
                            Unit = target,
                            Type = CoreType2
                        };

                        var poutput2 = SDKPrediction.GetPrediction(predInput2);

                        if (Spells.Speed != float.MaxValue &&
                            YasuoWindWall.CollisionYasuo(ObjectManager.Player.ServerPosition, poutput2.CastPosition))
                        {
                            return;
                        }

                        if (poutput2.Hitchance >= MinSDKHitChance)
                        {
                            Spells.Cast(poutput2.CastPosition, true);
                        }
                        else if (predInput2.AoE && poutput2.AoeTargetsHitCount > 1 &&
                                 poutput2.Hitchance >= MinSDKHitChance - 1)
                        {
                            Spells.Cast(poutput2.CastPosition, true);
                        }
                    }
                    break;
            }
        }

        public static void OktwCast(this Spell Spells, Obj_AI_Base target, bool AOE = false)
        {
            OktwPrediction.SkillshotType CoreType2 = OktwPrediction.SkillshotType.SkillshotLine;

            if (Spells.Type == SkillshotType.SkillshotCircle)
            {
                CoreType2 = OktwPrediction.SkillshotType.SkillshotCircle;
            }

            var predInput2 = new OktwPrediction.PredictionInput
            {
                Aoe = AOE,
                Collision = Spells.Collision,
                Speed = Spells.Speed,
                Delay = Spells.Delay,
                Range = Spells.Range,
                From = ObjectManager.Player.ServerPosition,
                Radius = Spells.Width,
                Unit = target,
                Type = CoreType2
            };

            var poutput2 = OktwPrediction.Prediction.GetPrediction(predInput2);

            if (Spells.Speed != float.MaxValue &&
                YasuoWindWall.CollisionYasuo(ObjectManager.Player.ServerPosition, poutput2.CastPosition))
            {
                return;
            }

            if (poutput2.Hitchance >= MinOKTWHitChance)
            {
                Spells.Cast(poutput2.CastPosition, true);
            }
            else if (predInput2.Aoe && poutput2.AoeTargetsHitCount > 1 && poutput2.Hitchance >= MinOKTWHitChance - 1)
            {
                Spells.Cast(poutput2.CastPosition, true);
            }
        }

        public static bool CheckTarget(Obj_AI_Base target, float range = float.MaxValue)
        {
            if (target == null)
            {
                return false;
            }

            if (target.DistanceToPlayer() > range)
            {
                return false;
            }

            if (target.HasBuff("KindredRNoDeathBuff"))
            {
                return false;
            }

            if (target.HasBuff("UndyingRage") && target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3)
            {
                return false;
            }

            if (target.HasBuff("JudicatorIntervention"))
            {
                return false;
            }

            if (target.HasBuff("ChronoShift") && target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3)
            {
                return false;
            }

            if (target.HasBuff("ShroudofDarkness"))
            {
                return false;
            }

            if (target.HasBuff("SivirShield"))
            {
                return false;
            }

            return !target.HasBuff("FioraW");
        }

        public static bool CheckTargetSureCanKill(Obj_AI_Base target)
        {
            if (target == null)
            {
                return false;
            }

            if (target.HasBuff("KindredRNoDeathBuff"))
            {
                return false;
            }

            if (target.HasBuff("UndyingRage") && target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3)
            {
                return false;
            }

            if (target.HasBuff("JudicatorIntervention"))
            {
                return false;
            }

            if (target.HasBuff("ChronoShift") && target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3)
            {
                return false;
            }

            if (target.HasBuff("ShroudofDarkness"))
            {
                return false;
            }

            if (target.HasBuff("SivirShield"))
            {
                return false;
            }

            return !target.HasBuff("FioraW");
        }

        public static double ComboDamage(Obj_AI_Hero target)
        {
            if (target != null && !target.IsDead && !target.IsZombie)
            {
                if (target.HasBuff("KindredRNoDeathBuff"))
                {
                    return 0;
                }

                if (target.HasBuff("UndyingRage") && target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3)
                {
                    return 0;
                }

                if (target.HasBuff("JudicatorIntervention"))
                {
                    return 0;
                }

                if (target.HasBuff("ChronoShift") && target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3)
                {
                    return 0;
                }

                if (target.HasBuff("FioraW"))
                {
                    return 0;
                }

                if (target.HasBuff("ShroudofDarkness"))
                {
                    return 0;
                }

                if (target.HasBuff("SivirShield"))
                {
                    return 0;
                }

                var damage = 0d;

                damage += ObjectManager.Player.GetAutoAttackDamage(target) +
                          (ObjectManager.Player.ChampionName == "Draven"
                              ? (Pluging.Draven.AxeCount > 0
                                  ? ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q)
                                  : 0)
                              : (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).IsReady()
                                  ? ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q)
                                  : 0d)) +
                          (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).IsReady()
                              ? ObjectManager.Player.GetSpellDamage(target, SpellSlot.W)
                              : 0d) +
                          (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).IsReady()
                              ? ObjectManager.Player.GetSpellDamage(target, SpellSlot.E)
                              : 0d) +
                          (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).IsReady()
                              ? ObjectManager.Player.GetSpellDamage(target, SpellSlot.R)
                              : 0d) +
                          ((ObjectManager.Player.GetSpellSlot("SummonerDot") != SpellSlot.Unknown &&
                            ObjectManager.Player.GetSpellSlot("SummonerDot").IsReady())
                              ? 50 + 20*ObjectManager.Player.Level - (target.HPRegenRate/5*3)
                              : 0d);

                if (target.ChampionName == "Moredkaiser")
                {
                    damage -= target.Mana;
                }

                if (ObjectManager.Player.HasBuff("SummonerExhaust"))
                {
                    damage = damage * 0.6f;
                }

                if (target.HasBuff("GarenW"))
                {
                    damage = damage * 0.7f;
                }

                if (target.HasBuff("ferocioushowl"))
                {
                    damage = damage * 0.7f;
                }

                if (target.HasBuff("BlitzcrankManaBarrierCD") && target.HasBuff("ManaBarrier"))
                {
                    damage -= target.Mana / 2f;
                }

                return damage;
            }

            return 0d;
        }


        public static bool CanMove(this Obj_AI_Hero Target)
        {
            return !(Target.MoveSpeed < 50) && !Target.IsStunned && !Target.HasBuffOfType(BuffType.Stun) &&
                   !Target.HasBuffOfType(BuffType.Fear) && !Target.HasBuffOfType(BuffType.Snare) &&
                   !Target.HasBuffOfType(BuffType.Knockup) && !Target.HasBuff("Recall") &&
                   !Target.HasBuffOfType(BuffType.Knockback)
                   && !Target.HasBuffOfType(BuffType.Charm) && !Target.HasBuffOfType(BuffType.Taunt) &&
                   !Target.HasBuffOfType(BuffType.Suppression) && (!Target.IsCastingInterruptableSpell()
                                                                   || Target.IsMoving) &&
                   !Target.HasBuff("zhonyasringshield") && !Target.HasBuff("bardrstasis");
        }           // zhonya                               // bard r

        public static float DistanceSquared(this Obj_AI_Base source, Vector3 position)
        {
            return source.DistanceSquared(position.To2D());
        }

        private static float DistanceSquared(this Obj_AI_Base source, Vector2 position)
        {
            return source.ServerPosition.DistanceSquared(position);
        }

        public static float DistanceSquared(this Vector3 vector3, Vector2 toVector2)
        {
            return vector3.To2D().DistanceSquared(toVector2);
        }

        public static float DistanceSquared(this Vector2 vector2, Vector2 toVector2)
        {
            return Vector2.DistanceSquared(vector2, toVector2);
        }

        public static float DistanceSquared(this Vector3 vector3, Vector3 toVector3)
        {
            return vector3.To2D().DistanceSquared(toVector3);
        }

        public static float DistanceSquared(this Vector2 vector2, Vector3 toVector3)
        {
            return Vector2.DistanceSquared(vector2, toVector3.To2D());
        }

        public static float DistanceSquared(this Vector2 point, Vector2 segmentStart, Vector2 segmentEnd,
            bool onlyIfOnSegment = false)
        {
            var objects = point.ProjectOn(segmentStart, segmentEnd);

            return objects.IsOnSegment || onlyIfOnSegment == false
                ? Vector2.DistanceSquared(objects.SegmentPoint, point)
                : float.MaxValue;
        }

        public static Vector2[] CircleCircleIntersection(this Vector2 center1, Vector2 center2, float radius1, float radius2)
        {
            var d = center1.Distance(center2);

            if (d > radius1 + radius2 || (d <= Math.Abs(radius1 - radius2)))
            {
                return new Vector2[] { };
            }

            var a = ((radius1 * radius1) - (radius2 * radius2) + (d * d)) / (2 * d);
            var h = (float)Math.Sqrt((radius1 * radius1) - (a * a));
            var direction = (center2 - center1).Normalized();
            var pa = center1 + (a * direction);
            var s1 = pa + h * direction.Perpendicular();
            var s2 = pa - h * direction.Perpendicular();

            return new[] { s1, s2 };
        }

        public static bool Compare(this GameObject gameObject, GameObject @object)
        {
            return gameObject != null && gameObject.IsValid && @object != null && @object.IsValid && gameObject.NetworkId == @object.NetworkId;
        }

        public static MovementCollisionInfo VectorMovementCollision( this Vector2 pointStartA, Vector2 pointEndA, float pointVelocityA, Vector2 pointB, float pointVelocityB, float delay = 0f)
        {
            return new[]
            {
                pointStartA,
                pointEndA }
            .VectorMovementCollision(pointVelocityA, pointB, pointVelocityB, delay);
        }

        private static MovementCollisionInfo VectorMovementCollision(this Vector2[] pointA, float pointVelocityA,
            Vector2 pointB, float pointVelocityB, float delay = 0f)
        {
            if (pointA.Length < 1)
            {
                return default(MovementCollisionInfo);
            }

            float sP1X = pointA[0].X,
                sP1Y = pointA[0].Y,
                eP1X = pointA[1].X,
                eP1Y = pointA[1].Y,
                sP2X = pointB.X,
                sP2Y = pointB.Y;
            float d = eP1X - sP1X, e = eP1Y - sP1Y;
            float dist = (float)Math.Sqrt((d * d) + (e * e)), t1 = float.NaN;
            float s = Math.Abs(dist) > float.Epsilon ? pointVelocityA*d/dist : 0,
                k = (Math.Abs(dist) > float.Epsilon) ? pointVelocityA*e/dist : 0f;

            float r = sP2X - sP1X, j = sP2Y - sP1Y;
            var c = (r * r) + (j * j);

            if (dist > 0f)
            {
                if (Math.Abs(pointVelocityA - float.MaxValue) < float.Epsilon)
                {
                    var t = dist / pointVelocityA;

                    t1 = pointVelocityB * t >= 0f ? t : float.NaN;
                }
                else if (Math.Abs(pointVelocityB - float.MaxValue) < float.Epsilon)
                {
                    t1 = 0f;
                }
                else
                {
                    float a = (s * s) + (k * k) - (pointVelocityB * pointVelocityB), b = (-r * s) - (j * k);

                    if (Math.Abs(a) < float.Epsilon)
                    {
                        if (Math.Abs(b) < float.Epsilon)
                        {
                            t1 = (Math.Abs(c) < float.Epsilon) ? 0f : float.NaN;
                        }
                        else
                        {
                            var t = -c / (2 * b);

                            t1 = (pointVelocityB * t >= 0f) ? t : float.NaN;
                        }
                    }
                    else
                    {
                        var sqr = (b * b) - (a * c);

                        if (sqr >= 0)
                        {
                            var nom = (float)Math.Sqrt(sqr);
                            var t = (-nom - b) / a;

                            t1 = pointVelocityB * t >= 0f ? t : float.NaN;
                            t = (nom - b) / a;

                            var t2 = (pointVelocityB * t >= 0f) ? t : float.NaN;

                            if (!float.IsNaN(t2) && !float.IsNaN(t1))
                            {
                                if (t1 >= delay && t2 >= delay)
                                {
                                    t1 = Math.Min(t1, t2);
                                }
                                else if (t2 >= delay)
                                {
                                    t1 = t2;
                                }
                            }
                        }
                    }
                }
            }
            else if (Math.Abs(dist) < float.Epsilon)
            {
                t1 = 0f;
            }

            return new MovementCollisionInfo(t1,
                !float.IsNaN(t1) ? new Vector2(sP1X + (s*t1), sP1Y + (k*t1)) : default(Vector2));
        }

        public struct MovementCollisionInfo
        {
            private readonly Vector2 CollisionPosition;
            private readonly float CollisionTime;

            internal MovementCollisionInfo(float collisionTime, Vector2 collisionPosition)
            {
                CollisionTime = collisionTime;
                CollisionPosition = collisionPosition;
            }

            public object this[int i] => i == 0 ? CollisionTime : (object)CollisionPosition;
        }

        public static float DistanceToPlayer(this Obj_AI_Base source)
        {
            return ObjectManager.Player.Distance(source);
        }

        public static float DistanceToPlayer(this Vector3 position)
        {
            return position.To2D().DistanceToPlayer();
        }

        private static float DistanceToPlayer(this Vector2 position)
        {
            return ObjectManager.Player.Distance(position);
        }

        public static float DistanceToMouse(this Vector3 position)
        {
            return position.To2D().DistanceToMouse();
        }

        private static float DistanceToMouse(this Vector2 position)
        {
            return Game.CursorPos.Distance(position.To3D());
        }
    }
}
