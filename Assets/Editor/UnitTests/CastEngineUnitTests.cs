using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

public class CastEngineUnitTests {

    JToken jsonAbility1 = JToken.Parse(@"{
                ""name"": ""Attack"",
		        ""castTime"": 1.15,
		        ""cooldownTime"": 1.85,
		        ""range"": 500,
		        ""effectsOnCast"": [
				        {
						        ""effectType"": ""damage"",
						        ""damageType"": ""piercing"",
						        ""targetStat"": ""hp_curr"",
						        ""valueBase"": 2,
						        ""valueStat"": ""str"",
						        ""valueMultiplier"": 2,
						        ""react"": ""shake""
                        }
		        ]
	        }");

    JToken jsonAbility2 = JToken.Parse(@"{
                ""name"": ""Heal"",
		        ""castTime"": 1.15,
		        ""cooldownTime"": 1.85,
		        ""range"": 500,
		        ""effectsOnCast"": [
				        {
						        ""effectType"": ""heal"",
						        ""targetStat"": ""hp_curr"",
						        ""valueBase"": 10,
						        ""valueStat"": ""int"",
						        ""valueMultiplier"": 2
                        }
		        ]
	        }");

    JToken jsonEntitySerialization = JToken.Parse(@"{
        ""name"":""bob"",
        ""stats"":{
            ""hp_curr"":85,
            ""hp_base"":125,
            ""xp_curr"":25,
            ""xp_level"":2,
            ""agi_base"":12,
            ""agi_curr"":12
        },
        ""inventory"":{
            ""bag"":[]
        }
    }");

	class TestCastPhysics : ICastPhysics
	{
		public Vector3? GetVecBetween(ICastEntity fromEntity, ICastEntity toEntity )
		{
            // all non TestCastEntities are at position '0,0,0' ie: EntityModels
            Vector3 pf = new Vector3();
            Vector3 pt = new Vector3();
            TestCastEntity f = fromEntity as TestCastEntity;
            if (f != null) pf = f.pos;

			TestCastEntity t = toEntity as TestCastEntity;
            if (t != null) pt = t.pos;

			return pt - pf;
		}
			
		// in: ICastEntity entity
		// out: null or Vec2D pos
		public Vector3? GetEntityPosition(ICastEntity entity )
		{
			if (entity == null)
				return null;
			TestCastEntity e = entity as TestCastEntity;
			return e.pos;
		}

		// in: Vec2D p, float r, array[ICastEntity] ignoreEntities
		// out: array<ICastEntity> entities
		public List<ICastEntity> GetEntitiesInRadius(Vector3 p, float r, List<ICastEntity> ignoreEntities = null )
		{
			//CastWorldModel.Get(). //xxx todo
			return new List<ICastEntity>();
		}
	}

	protected void __cleanTestEnvironment__() 
	{
		CastWorldModel.Reset();
		CastCommandTime.Set (0);
        CastCommandScheduler.Reset();

		TestCastPhysics physics = new TestCastPhysics ();
		CastWorldModel.Get ().setPhysicsInterface (physics);
	}

    [Test]
    public void testEntityCreationAndDestruction()
    {
		__cleanTestEnvironment__ ();

		CastWorldModel world = CastWorldModel.Get ();
        EntityModel entity = new EntityModel("model1");

        Assert.IsTrue(world.CountEntities() == 0);

        world.AddEntity(entity);

        Assert.IsTrue(world.CountEntities() == 1);

        world.RemoveEntity(entity);

        Assert.IsTrue(world.CountEntities() == 0);

        entity.Destroy();
    }

    [Test]
    public void testEntityDeserialization()
    {
		__cleanTestEnvironment__ ();

		EntityModel entity = new EntityModel("model2");
        entity.initFromJson(jsonEntitySerialization);
        Assert.IsTrue(entity.hp_base == (int)jsonEntitySerialization["stats"]["hp_base"]);
        Assert.IsTrue(entity.hp_curr == (int)jsonEntitySerialization["stats"]["hp_curr"]);
        entity.Destroy(); //if we dont clean up, testEntityCreationAndDestruction will fail!
    }

    [Test]
    public void testAbilityCreation()
    {
		__cleanTestEnvironment__ ();

        CastCommandModel model = new CastCommandModel(jsonAbility1);
        CastCommandState instance = new CastCommandState(model, null);

        Assert.IsNotNull(instance);
    }

	class TestCastEntity : ICastEntity
	{
		CastTarget castTarget = new CastTarget();
		public Vector3 pos = new Vector3();

		public TestCastEntity() {
			CastWorldModel.Get().AddEntity(this);
		}
		public void Destroy() {
			CastWorldModel.Get ().RemoveEntity (this);
		}
		public void setProperty(string propName, float value, CastEffect effect) {}
		public void incProperty(string propName, float value, CastEffect effect) {}
		public void startBuffProperty(string propName, float value, CastEffect effect) {}
		public void endBuffProperty(string propName, float value, CastEffect effect) {}

		// in: string propName
		//float
		public float getProperty(string propName) { return 0; }

		public void testSetTarget( ICastEntity target ) {
			castTarget.addTargetEntity (target);
		}
		//CastTarget
		public CastTarget getTarget() { return castTarget; }

		// in: json reaction, CastEffect source
		public void handleEffectReaction(JToken reaction, CastEffect source) {}

		public void handleEffectEvent(string effectEventName, CastEffect source) {}

		//effect is ARRIVING at this entity
		public void applyEffect(CastEffect effect) {}
		public void removeEffect(CastEffect effect) {}
	}

    //todo: test ability use
	[Test]
	public void testAbilityUse() 
	{
		__cleanTestEnvironment__ ();

		CastCommandScheduler scheduler = CastCommandScheduler.Get ();

		CastCommandModel model = new CastCommandModel(jsonAbility1);

		TestCastEntity entity = new TestCastEntity ();
		TestCastEntity target = new TestCastEntity ();
		entity.testSetTarget (target);
		CastCommandState instance = new CastCommandState(model, entity);

		Assert.IsFalse (instance.isCasting ());

		Assert.IsTrue (instance.startCast ());

		Assert.IsTrue (instance.isCasting ());

		CastCommandTime.UpdateDelta (0.1);
		scheduler.Update ();

		Assert.IsTrue (instance.isCasting ());

		CastCommandTime.UpdateDelta (1.1);
		scheduler.Update ();

		Assert.IsFalse (instance.isCasting ());
		Assert.IsTrue (instance.isOnCooldown ());

        CastCommandTime.UpdateDelta(0.1);
        scheduler.Update();

        Assert.IsFalse(instance.isIdle());
        Assert.IsTrue(instance.isOnCooldown());

        CastCommandTime.UpdateDelta(3.1);
        scheduler.Update();

        Assert.IsTrue(instance.isIdle());
        Assert.IsFalse(instance.isOnCooldown());

        entity.Destroy ();
		target.Destroy ();
	}

    [Test]
    public void testAbilityDamage()
    {
        __cleanTestEnvironment__ ();

        CastCommandScheduler scheduler = CastCommandScheduler.Get();
        CastWorldModel world = CastWorldModel.Get();

        TestCastEntity entity = new TestCastEntity();
        EntityModel target = new EntityModel("targ1");
        world.AddEntity(target);
        entity.testSetTarget(target);

        CastCommandModel model = new CastCommandModel(jsonAbility1);
        CastCommandState instance = new CastCommandState(model, entity);

        Assert.IsTrue(target.hp_curr == 100);

        Assert.IsTrue(instance.startCast());

        CastCommandTime.UpdateDelta(1.2);
        scheduler.Update();

        Assert.IsTrue(instance.isOnCooldown());

        Assert.IsFalse(target.hp_curr == 100);

        world.RemoveEntity(target);

        entity.Destroy();
        target.Destroy();
    }

    [Test]
    public void testAbilityHeal()
    {
        __cleanTestEnvironment__();

        CastCommandScheduler scheduler = CastCommandScheduler.Get();
        CastWorldModel world = CastWorldModel.Get();

        TestCastEntity entity = new TestCastEntity();
        EntityModel target = new EntityModel("targ1");
        world.AddEntity(target);
        entity.testSetTarget(target);

        CastCommandModel model = new CastCommandModel(jsonAbility2);
        CastCommandState instance = new CastCommandState(model, entity);

        target.setProperty("hp_curr", 50, null);
        Assert.IsTrue(target.hp_curr == 50);

        Assert.IsTrue(instance.startCast());

        CastCommandTime.UpdateDelta(1.2);
        scheduler.Update();

        Assert.IsTrue(instance.isOnCooldown());

        Assert.IsTrue(target.hp_curr == 60);

        world.RemoveEntity(target);

        entity.Destroy();
        target.Destroy();
    }
}
