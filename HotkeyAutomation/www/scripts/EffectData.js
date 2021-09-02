export var EffectType = {
	HttpGet: "HttpGet",
	BroadLink: "BroadLink",
	iTach: "iTach",
	Vera: "Vera",
	HomeAssistant: "HomeAssistant"
};
export var VeraService = {
	DimmerValue: "DimmerValue",
	SwitchSet: "SwitchSet",
	CurtainStop: "CurtainStop"
};
export var HomeAssistantMethod = {
	DimmerValue: "DimmerValue",
	SwitchSet: "SwitchSet"
};
export class Effect
{
	constructor()
	{
		this.type = EffectType.HttpGet;
		this.data = new EffectData();
	}
}
export class EffectData
{
	constructor()
	{
		this.httpget_url = null;
		this.broadlink_name = null;
		this.broadlink_commandName = null;
		this.broadlink_repeatCount = 0;
		this.itach_name = null;
		this.itach_connectorAddress = null;
		this.itach_commandShortName = null;
		this.itach_repeatCount = 0;
		this.vera_name = null;
		this.vera_deviceNum = 0;
		this.vera_service = VeraService.DimmerValue;
		this.vera_value = null;
		this.hass_servername = null;
		this.hass_entityid = null;
		this.hass_method = null;
		this.hass_value = null;
	}
}