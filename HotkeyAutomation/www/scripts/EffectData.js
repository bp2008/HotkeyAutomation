export var EffectType = {
	HttpGet: "HttpGet",
	iTach: "iTach",
	Vera: "Vera"
};
export var VeraService = {
	DimmerValue: "DimmerValue",
	SwitchSet: "SwitchSet",
	CurtainStop: "CurtainStop"
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
		this.itach_name = null;
		this.itach_connectorAddress = null;
		this.itach_commandShortName = null;
		this.itach_repeatCount = 0;
		this.vera_name = null;
		this.vera_deviceNum = 0;
		this.vera_service = VeraService.DimmerValue;
		this.vera_value = null;
	}
}