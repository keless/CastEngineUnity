# CastEngineUnity
Unity3D implementation of CastEngine

CastEngine is a resuable data-driven library for enabling common game mechanics like "spells, attacks and abilities"

Anything that could have a 'cast time' a 'channel time' a 'cooldown' and applies 'effects' either once, or repeatedly over time (think HoTs/DoTs), can make use of this.

The abilities themselves are specified by JSON blobs and can freely be changed and extended (with the original goal to even let users create their own spells).

The engine is data-model only; you have to implement interfaces that will actually apply the effects to your entities and trigger visuals and animations specific to your game.

To prove the engine out, a small demo game is included which makes use of CastEngine.
