using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public interface ICastEntity
{
    /*
    public ICastEntity()
    {
        CastWorldModel.Get().AddEntity(this);
    }

    public void Destroy()
    {
        CastWorldModel.Get().RemoveEntity(this);
    }
    */
    /// <summary>
    /// called when object should be cleaned up
    /// </summary>
    void Destroy();

    /// <summary>
    /// set value to an entity's property
    /// </summary>
    /// <param name="propName">name of property</param>
    /// <param name="value">value to set property to</param>
    /// <param name="effect">effect that caused the setProperty call (or null)</param>
    void setProperty(string propName, float value, CastEffect effect);
    void incProperty(string propName, float value, CastEffect effect);
    void startBuffProperty(string propName, float value, CastEffect effect);
    void endBuffProperty(string propName, float value, CastEffect effect);

    // in: string propName
    //float
    float getProperty(string propName);

    //CastTarget
    CastTarget getTarget();

    // in: json reaction, CastEffect source
    void handleEffectReaction(JToken reaction, CastEffect source);

    void handleEffectEvent(string effectEventName, CastEffect source);

    //effect is ARRIVING at this entity
    void applyEffect(CastEffect effect);
    void removeEffect(CastEffect effect);

    void onAbilityCastStart(CastCommandState ability);
    void onAbilityChannelStart(CastCommandState ability);
    void onAbilityCooldownStart(CastCommandState ability);
    void onAbilityIdleStart(CastCommandState ability);
}
