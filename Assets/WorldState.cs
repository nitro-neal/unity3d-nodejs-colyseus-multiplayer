// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.3
// 

using Colyseus.Schema;

public class WorldState : Schema
{
    [Type(0, "map", typeof(MapSchema<MyPlayer>))]
    public MapSchema<MyPlayer> players = new MapSchema<MyPlayer>();
}

