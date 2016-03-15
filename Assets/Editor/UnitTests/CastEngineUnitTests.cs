using UnityEngine;
using System.Collections;
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

    [Test]
    public void testEntityCreationAndDestruction()
    {
        CastWorldModel world = CastWorldModel.Get();

        EntityModel entity = new EntityModel("model1");

        Assert.IsTrue(world.CountEntities() == 1);

        entity.Destroy();

        Assert.IsTrue(world.CountEntities() == 0);
    }

    [Test]
    public void testEntityDeserialization()
    {
        EntityModel entity = new EntityModel("model1");
        entity.initFromJson(jsonEntitySerialization);
        Assert.IsTrue(entity.hp_base == (int)jsonEntitySerialization["stats"]["hp_base"]);
        Assert.IsTrue(entity.hp_curr == (int)jsonEntitySerialization["stats"]["hp_curr"]);
        entity.Destroy(); //if we dont clean up, testEntityCreationAndDestruction will fail!
    }

    [Test]
    public void testAbilityCreation()
    {
        CastCommandModel model = new CastCommandModel(jsonAbility1);
        CastCommandState instance = new CastCommandState(model, null);
    }

    //todo: test ability use

}
