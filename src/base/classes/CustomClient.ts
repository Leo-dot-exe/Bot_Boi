import { Client } from "discord.js";
import "dotenv/config";

import ICustomClient from "../interfaces/ICustomClient";
import IConfig from "../interfaces/IConfig";


export default class CustomClient extends Client implements ICustomClient {

  config: IConfig;
  discord_token: string;

  constructor() {
    super({ intents: [] });

    this.config = require(`${process.cwd()}/data/config.json`)
    this.discord_token = process.env.BOT_TOKEN || "null";
  }

  init(): void {
    this.login(this.discord_token)
      .then(() => console.log("BOT LOGGED IN"))
      .catch((err: Error) => { throw new Error(err.message) })
  }

}