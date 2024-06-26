﻿using GXPEngine;
using System;
using TiledMapParser;

public class GravityObject : AnimationSprite
{
    bool landed = false;
    public float radius;
    public Vec2GravityCollider gCollider;
    public GravityObject(string filename, int cols, int rows, TiledObject obj) : base (filename, cols, rows)
    {
        radius = obj.GetFloatProperty("radius", width/2);

        gCollider = new Vec2GravityCollider(new Vec2BallCollider(radius, obj.GetFloatProperty("density", 1), obj.GetBoolProperty("stationary", true)));
        rotation = 0;
        Vec2 gColPos = new Vec2(obj.X + obj.Width / 2, obj.Y - obj.Height / 2);
        gColPos.RotateAroundDegrees(new Vec2(obj.X, obj.Y), obj.Rotation);
        gCollider._collider._position = gColPos;

        

        x = gCollider._collider._position.x;
        y = gCollider._collider._position.y;
    }

    public virtual void Draw() { }

    public virtual void UpdateScreenPosition()
    {
        x = gCollider._collider._position.x;
        y = gCollider._collider._position.y;
        Draw();
    }

    public virtual void Step()
    {
        Planet nearestPlanet = FindNearestPlanet();
        Vec2Collider colider = gCollider._collider;
        if (nearestPlanet == null) return;
        Vec2Collider planetColider = nearestPlanet.gCollider._collider;
        if (Vec2PhysicsCalculations.TimeOfImpactBall(colider._velocity, colider._position, colider.GetOldPosition(), planetColider._position, radius, nearestPlanet.radius) < 0.0001f ||
            (colider._position.distance(planetColider._position)-(radius+nearestPlanet.radius)) <= 0.5f)
        {
            float aproachSpeed = colider._velocity.Dot((planetColider._position - colider._position).Normalized());

            //Console.WriteLine(aproachSpeed);

            Vec2 planetAngle = planetColider._position - colider._position;

            if (aproachSpeed>0 && nearestPlanet.gCollider._collider.mass > 0) colider._position = planetColider._position + (planetAngle.Normalized() * -(radius + nearestPlanet.radius));

            if (nearestPlanet.gCollider._collider.mass>0)colider._velocity = new Vec2(0, 0);
            if (!landed && this is Player)
            {
                SoundHandler.Landing.Play();
                landed = true;
            }
        } else landed = false;
    }

    public Planet FindNearestPlanet()
    {
        float nearestdistance = float.PositiveInfinity;
        Planet nearestPlanet = null;
        foreach (Planet p in MyGame.planets) 
        {
            float distance = (gCollider._collider._position - p.gCollider._collider._position).Length() - (radius+p.radius);

            if (distance < nearestdistance)
            {
                nearestPlanet = p;
                nearestdistance = distance;
            }
        }

        return nearestPlanet;
    }
}
