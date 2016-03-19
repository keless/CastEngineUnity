using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class test : MonoBehaviour {

    // Use this for initialization
    void Start() {
        //Test();
    }

    void Test() { 
        string strJson1 = "{ \"blah\":5 }";
        string strJson2 = @"{
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
                        },
				        {
						        ""effectType"": ""healing"",
						        ""targetStat"": ""hp_curr"",
						        ""valueBase"": 2,
						        ""valueStat"": ""int"",
						        ""valueMultiplier"": 2
                        }
		        ]
	        }";

        JSONObject j = new JSONObject(strJson1);
        Debug.Log("j: - " + j);
        Debug.Log("j['blah']: 5 -  " + j["blah"]);
        j = new JSONObject(strJson2);
        if(j["effectsOnCast"])
        {
            Debug.Log("j: effectsOnCast length: " + j["effectsOnCast"].Count);

            float valueBase = j["effectsOnCast"][0]["valueBase"].f;
            Debug.Log("j: effectsOnCast[0].valueBase " + valueBase);

            foreach( JSONObject eff in j["effectsOnCast"])
            {
                string react; // = eff["react"].str ?? "no reaction";
                eff.GetField(out react, "react", "no reaction");
                Debug.Log("j: react type - " + react);

                if (!eff["damageType"]) continue;
                string damageType = eff["damageType"].str;
                Debug.Log("j: damage type " + damageType);
            }
        }
        else
        {
            Debug.Log("j: effectsOnCast DNE ");
        }

        

        JToken n = JToken.Parse(strJson1);
        Debug.Log("n: -  " + n);
        Debug.Log("n['blah']: 5 -  " + n["blah"]);
        n = JToken.Parse(strJson2);
        if(n["effectsOnCast"] != null)
        {
            Debug.Log("n: effectsOnCast length: " + n.Get<JArray>("effectsOnCast").Count);

            float valueBase = (float) n["effectsOnCast"][0]["valueBase"];
            Debug.Log("n: effectsOnCast[0].valueBase " + valueBase);

            foreach( JToken eff in n["effectsOnCast"])
            {
                string react = eff.Get<string>("react") ?? "no reaction";
                Debug.Log("n: react type - " + react);

                if (!eff.hasOwnMember("damageType")) continue;
                string damageType = (string)eff["damageType"];
                Debug.Log("n: damage type " + damageType);
            }
        }
        else
        {
            Debug.Log("n: effectsOnCast DNE ");
        }
    }

}
