import { Room, Client } from "colyseus";
import { Schema, MapSchema, type } from "@colyseus/schema";

// An abstract player object, demonstrating a potential 2D world position
export class MyPlayer extends Schema {
    @type("number")
    x: number = 0.11;

    @type("number")
    y: number = 2.22;

    @type("number")
    color: number = 777;
}

// Our custom game state, an ArraySchema of type Player only at the moment
export class WorldState extends Schema {
    @type({ map: MyPlayer })
    players = new MapSchema<MyPlayer>();
}

export class AmongUsRoom extends Room<WorldState> {
    // Colyseus will invoke when creating the room instance
    onCreate(options: any) {
        // initialize empty room state
        this.setState(new WorldState());

        // Called every time this room receives a "move" message
        this.onMessage("move", (client, data) => {
            const player = this.state.players.get(client.sessionId);
            player.x = data.x;
            player.y = data.y;
            console.log(client.sessionId + " at, x: " + player.x, "y: " + player.y);
        });
    }

    // Called every time a client joins
    onJoin(client: Client, options: any) {
        this.state.players.set(client.sessionId, new MyPlayer());
    }
}