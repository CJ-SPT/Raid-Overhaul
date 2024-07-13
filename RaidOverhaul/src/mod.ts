import { DependencyContainer } from "tsyringe"

import { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod"
import { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod"
import { ITraderConfig } from "@spt/models/spt/config/ITraderConfig"
import { IRagfairConfig } from "@spt/models/spt/config/IRagfairConfig"
import { LogTextColor } from "@spt/models/spt/logging/LogTextColor"
import { Traders } from "@spt/models/enums/Traders"
import { ConfigTypes } from "@spt/models/enums/ConfigTypes"
import { StaticRouterModService } from "@spt/services/mod/staticRouter/StaticRouterModService"
import { DynamicRouterModService } from "@spt/services/mod/dynamicRouter/DynamicRouterModService"
import { ImageRouter } from "@spt/routers/ImageRouter"

import { References } from "./Refs/References"
import { Utils } from "./Refs/Utils"
import { configFile } from "./Refs/Enums"
import { LegionData } from "./RaidBoss/Legion"
import { TraderData } from "./Trader/ReqShop"
import { Base } from "./BaseFeatures/baseFeatures"
import { ItemGenerator } from "./CustomItems/ItemGenerator"
import { pushTraderFeatures } from "./Trader/TraderPushes"

const legionClothes = require("../db/ItemGen/Clothes/LegionClothing.json")
const EventWeightingsConfig = require("../config/EventWeightings.json")

import * as baseJson from "../db/base.json"
import * as path from "node:path"
import * as fs from "node:fs"

class RaidOverhaul implements IPreSptLoadMod, IPostDBLoadMod {
	static modName: string
	protected profilePath: string

	static container: DependencyContainer
	public imageRouter: ImageRouter

	private ref: References = new References()
	private utils: Utils = new Utils(this.ref)

	private static pluginDepCheck(): boolean {
		const pluginRO = "raidoverhaul.dll"

		try {
			const pluginPath = fs.readdirSync("./BepInEx/plugins/RaidOverhaul").map((plugin) => plugin.toLowerCase())
			return pluginPath.includes(pluginRO)
		} catch {
			return false
		}
	}

	private static preloaderDepCheck(): boolean {
		const prePatchLegion = "legionpreloader.dll"

		try {
			const pluginPath = fs.readdirSync("./BepInEx/patchers").map((plugin) => plugin.toLowerCase())
			return pluginPath.includes(prePatchLegion)
		} catch {
			return false
		}
	}

	constructor() {
		RaidOverhaul.modName = "RaidOverhaul"
	}

	public preSptLoad(container: DependencyContainer): void {
		this.ref.preSptLoad(container)
		const ragfair = this.ref.configServer.getConfig<IRagfairConfig>(ConfigTypes.RAGFAIR)
		const traderConfig: ITraderConfig = this.ref.configServer.getConfig<ITraderConfig>(ConfigTypes.TRADER)
		const traderData = new TraderData(traderConfig, this.ref, this.utils)

		const staticRouterModService: StaticRouterModService = container.resolve<StaticRouterModService>("StaticRouterModService")
		const dynamicRouterModService: DynamicRouterModService = container.resolve<DynamicRouterModService>("DynamicRouterModService")
		const configPath = path.resolve(__dirname, "../config/config.json")
		const modConfig = this.ref.jsonUtil.deserialize(fs.readFileSync(configPath, "utf-8"), "config.json") as configFile
		const modFeatures = new Base(this.utils, this.ref)

		if (modConfig.RemoveFromSwag) {
			return
		}

		traderData.registerProfileImage()
		traderData.setupTraderUpdateTime()

		Traders["Requisitions"] = "Requisitions"
		ragfair.traders[baseJson._id] = true

		//Backup profile
		staticRouterModService.registerStaticRouter(
			`${RaidOverhaul.modName}-/client/game/start`,
			[
				{
					url: "/client/game/start",
					action: async (url: string, info: string, sessionID: string, output: string) => {
						const profileInfo = this.ref.profileHelper.getFullProfile(sessionID)

						if (modConfig.BackupProfile) {
							this.utils.profileBackup(RaidOverhaul.modName, sessionID, path, profileInfo, this.ref.randomUtil)
						}
						return output
					},
				},
			],
			"spt"
		)

		//Get and send configs to the client
		staticRouterModService.registerStaticRouter(
			"GetEventConfig",
			[
				{
					url: "/RaidOverhaul/GetEventConfig",
					action: async (url: string, info: string, sessionId: string, output: string) => {
						const EventWeightings = EventWeightingsConfig

						return JSON.stringify(EventWeightings)
					},
				},
			],
			""
		)

		staticRouterModService.registerStaticRouter(
			"GetServerConfig",
			[
				{
					url: "/RaidOverhaul/GetServerConfig",
					action: async (url: string, info: string, sessionId: string, output: string) => {
						const ServerConfig = modConfig

						return JSON.stringify(ServerConfig)
					},
				},
			],
			""
		)

		//Randomize weather pre-raid
		staticRouterModService.registerStaticRouter(
			"GetRandomizedWeather",
			[
				{
					url: "/client/raid/configuration",
					action: async (url, info, sessionId, output) => {
						modFeatures.weatherChanges(modConfig)
						return output
					},
				},
			],
			""
		)

		//Log from the client to the server if in debug build in the client and extra debug logging is enabled in the server
		if (modConfig.Debug.ExtraLogging) {
			dynamicRouterModService.registerDynamicRouter(
				`DynamicReportError${RaidOverhaul.modName}`,
				[
					{
						url: "/RaidOverhaul/LogToServer/",
						action: async (url: string) => {
							const urlParts = url.split("/")
							const clientMessage = urlParts[urlParts.length - 1]

							const regex = /%20/g
							this.utils.logToServer(clientMessage.replace(regex, " "), this.ref.logger)

							return JSON.stringify({ resp: "OK" })
						},
					},
				],
				"LogToServer"
			)
		}

		//Modify trader rep and legion chance post raid
		staticRouterModService.registerStaticRouter(
			`${RaidOverhaul.modName}:RaidSaved`,
			[
				{
					url: "/raid/profile/save",
					action: async (url: string, info: string, sessionId: string, output: string) => {
						TraderData.traderRepLogic(info, sessionId, this.ref.traderHelper)
						if (modConfig.EnableCustomBoss) {
							TraderData.legionRepLogic(info, sessionId, this.ref.traderHelper)
							LegionData.modifySpawnChance(info, output)
							LegionData.LoadBossData(modConfig)
							if (this.ref.preSptModLoader.getImportedModsNames().includes("SWAG")) {
								LegionData.swagPatch()
							}
						}
						if (!modConfig.EnableCustomBoss) {
							TraderData.noBossRepLogic(info, sessionId, this.ref.traderHelper)
						}
						return output
					},
				},
			],
			"spt"
		)

		//Patch Legion into SWAG patterns

		if (modConfig.EnableCustomBoss) {
			if (this.ref.preSptModLoader.getImportedModsNames().includes("SWAG")) {
				LegionData.swagPatch()
				this.ref.logger.logWithColor("[RaidOverhaul] SWAG detected, modifying Legion patterns.", LogTextColor.MAGENTA)
			}
		}

		//Remap ID's to the new version in the profile. Deprecated in 390 since you should have a new profile anyways and all ID's are updated
		//this.utils.fixOldROIds()
	}

	public postDBLoad(container: DependencyContainer): void {
		this.ref.postDBLoad(container)

		const traderConfig: ITraderConfig = this.ref.configServer.getConfig<ITraderConfig>(ConfigTypes.TRADER)

		//Imports
		const traderData = new TraderData(traderConfig, this.ref, this.utils)
		const modFeatures = new Base(this.utils, this.ref)
		const itemGenerator = new ItemGenerator(this.ref)
		const traderFeatures = new pushTraderFeatures(this.utils, this.ref, traderData)

		//For new items
		const modPath = path.resolve(__dirname.toString()).split(path.sep).join("/") + "/"
		const configPath = path.resolve(__dirname, "../config/config.json")
		const modConfig = this.ref.jsonUtil.deserialize(fs.readFileSync(configPath, "utf-8"), "config.json") as configFile

		//Random message on server on startup
		const messageArray = [
			"The hamsters can take a break now",
			"Time to get wrecked by Birdeye LOL",
			"Back to looking for cat pics",
			"I made sure to crank up your heart attack event chances",
			"If there's a bunch of red text it's 100% not my fault",
			"We are legion, for we are many",
			"All Hail the Cult of Cj",
			"Good luck out there",
		]
		const randomMessage = messageArray[Math.floor(Math.random() * messageArray.length)]

		//Remove boss from SWAG
		if (modConfig.RemoveFromSwag) {
			const swagBossConfigPath = path.join(__dirname, "../../SWAG/config/bossConfig.json")
			const swagBossConfig = JSON.parse(fs.readFileSync(swagBossConfigPath, "utf-8"))

			swagBossConfig.CustomBosses.legion.enabled = false

			fs.writeFileSync(swagBossConfigPath, JSON.stringify(swagBossConfig, null, 2), "utf-8")

			return this.ref.logger.error(`[${RaidOverhaul.modName}] Removing Legion from Swag config. Ready to uninstall.`)
		}

		//Check for proper install
		if (!RaidOverhaul.pluginDepCheck()) {
			return this.ref.logger.error(
				`[${RaidOverhaul.modName}] Error, client portion of Raid Overhaul is missing from BepInEx/plugins folder.\nPlease install correctly.`
			)
		}

		if (!RaidOverhaul.preloaderDepCheck()) {
			return this.ref.logger.error(`[${RaidOverhaul.modName}] Error, Legion Boss Preloader is missing from BepInEx/patchers folder.\nPlease install correctly.`)
		}

		//Load all custom items
		itemGenerator.createCustomItems("../../db/ItemGen/Currency")
		itemGenerator.createCustomItems("../../db/ItemGen/ConstItems")
		itemGenerator.createClothingTop(legionClothes.Shirt)
		itemGenerator.createClothingBottom(legionClothes.Pants)
		if (modConfig.EnableCustomItems) {
			if (this.ref.preSptModLoader.getImportedModsNames().includes("SPT-Realism")) {
				itemGenerator.createCustomItems("../../db/ItemGen/Ammo Realism")
				this.ref.logger.logWithColor("[RaidOverhaul] Realism detected, modifying custom ammunition.", LogTextColor.MAGENTA)
			}

			if (!this.ref.preSptModLoader.getImportedModsNames().includes("SPT-Realism")) {
				itemGenerator.createCustomItems("../../db/ItemGen/Ammo")
			}
			itemGenerator.createCustomItems("../../db/ItemGen/Weapons")
			itemGenerator.createCustomItems("../../db/ItemGen/Gear")
		}

		// Load custom boss data
		if (modConfig.EnableCustomBoss) {
			LegionData.LoadBossData(modConfig)
			traderFeatures.pushExports(modPath, modConfig)
		}

		if (!modConfig.EnableCustomBoss) {
			traderFeatures.pushExports2(modPath, modConfig)
		}

		// Load Trader Data
		traderFeatures.buildReqAssort(modConfig)

		//Push all of the mods base features
		modFeatures.raidChanges(modConfig)
		modFeatures.itemChanges(modConfig)
		modFeatures.lootChanges(modConfig)
		modFeatures.stackChanges(modConfig)
		modFeatures.traderTweaks(modConfig)
		modFeatures.eventChanges(modConfig)
		modFeatures.weatherChanges(modConfig)
		modFeatures.weightChanges(modConfig)
		modFeatures.addReqShopInsurance()

		this.ref.logger.logWithColor(`[${RaidOverhaul.modName}] has finished modifying your raids. ${randomMessage}.`, LogTextColor.CYAN)
	}
}
//      \('_')/     \('_')/     \('_')/     \('_')/     \('_')/     \('_')/     \('_')/     \('_')/     \('_')/     \('_')/     \('_')/     \('_')/     \('_')/     \('_')/

module.exports = { mod: new RaidOverhaul() }
