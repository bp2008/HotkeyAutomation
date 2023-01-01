<template>
	<div>
		<!-- IMPORTANT: Any fields of "effect" must be initialized in scripts/EffectData.js -->
		<div v-if="effect" class="effectRoot">
			<div class="buttons">
				<span title="Drag me!" class="hotkeyEffect_dragHandle"><svg><use xlink:href="#arrows"></use></svg></span>
				<span title="Delete this effect"><svg @click="removeClick"><use xlink:href="#remove"></use></svg></span>
			</div>
			<div>
				<label>
					Effect Type:
					<VSelect v-model="effect.type" :options="effectTypes" @change="editEffectType" />
				</label>
			</div>
			<div v-if="effect.type === EffectType.HttpGet" key="httpget">
				<label class="wide">URL: <input type="text" v-model="effect.data.httpget_url" @change="edit" /></label>
			</div>
			<div v-if="effect.type === EffectType.HttpPost" key="httppost">
				<label class="wide">URL: <input type="text" v-model="effect.data.httppost_url" @change="edit" /></label>
				<label class="wide" title="If empty, defaults to &quot;application/x-www-form-urlencoded&quot;">Content-Type: <input type="text" v-model="effect.data.httppost_content_type" @change="edit" placeholder="application/x-www-form-urlencoded" /></label>
				<label class="wide">POST Body: <input type="text" v-model="effect.data.httppost_body" @change="edit" /></label>
			</div>
			<div v-else-if="effect.type === EffectType.BroadLink" key="broadlink">
				<label>BroadLink Name: <VSelect v-model="effect.data.broadlink_name" :options="broadlinkNameOptions" @change="edit" /></label>
				<label>Command Name: <VSelect v-model="effect.data.broadlink_commandName" :options="broadlinkCommandOptions" @change="edit" /></label>
				<label>Repeat Count: <input type="number" v-model.number="effect.data.broadlink_repeatCount" min="0" max="255" @change="edit" /></label>
			</div>
			<div v-else-if="effect.type === EffectType.iTach" key="itach">
				<label>iTach Name: <VSelect v-model="effect.data.itach_name" :options="itachNameOptions" @change="edit" /></label>
				<label>Connector Address: <input type="text" v-model="effect.data.itach_connectorAddress" @change="edit" placeholder="e.g. 1:1 or 1:2 or 1:3" /></label>
				<label>Command Name: <VSelect v-model="effect.data.itach_commandShortName" :options="irCommandOptions" @change="edit" /></label>
				<label>Repeat Count: <input type="number" v-model.number="effect.data.itach_repeatCount" min="0" max="50" @change="edit" /></label>
			</div>
			<div v-else-if="effect.type === EffectType.Vera" key="vera">
				<label>Vera Name: <VSelect v-model="effect.data.vera_name" :options="veraNameOptions" @change="edit" /></label>
				<label>Device: <VSelect v-model="effect.data.vera_deviceNum" :options="veraDeviceOptions" @change="edit" /></label>
				<label>
					Service Type:
					<VSelect v-model="effect.data.vera_service" :options="veraServices" @change="edit" />
				</label>
				<label>Value: <input type="text" v-model="effect.data.vera_value" @change="edit" /></label>
			</div>
			<div v-else-if="effect.type === EffectType.HomeAssistant" key="homeassistant">
				<label>HomeAssistant Server: <VSelect v-model="effect.data.hass_servername" :options="homeAssistantNameOptions" @change="edit" /></label>
				<label>Device: <VSelect v-model="effect.data.hass_entityid" :options="homeAssistantEntities" @change="edit" /></label>
				<label>
					Method:
					<VSelect v-model="effect.data.hass_method" :options="homeAssistantMethods" @change="edit" />
				</label>
				<label v-if="homeAssistantEffectRange">
					Value: <input type="number" v-model="effect.data.hass_value" @change="edit"
							 :min="homeAssistantEffectRange[0]"
							 :max="homeAssistantEffectRange[1]" /><span> ({{homeAssistantEffectRangeStr}})</span>
				</label>
			</div>
			<div v-else key="unknownEffectType">
				Unknown Effect Type: {{effect.type}}
			</div>
		</div>
	</div>
</template>

<script>
	import svg1 from 'appRoot/images/sprite/remove.svg';
	import VSelect from 'appRoot/vues/common/controls/VSelect.vue';
	import { EffectType, VeraService, HomeAssistantMethod } from 'appRoot/scripts/EffectData.js';

	export default {
		components: { VSelect },
		props:
		{
			effect: { // IMPORTANT: Any fields of "effect" must be initialized in scripts/EffectData.js
				type: Object,
				required: true
			}
		},
		data()
		{
			return {
				EffectType: EffectType,
				VeraService: VeraService,
				HomeAssistantMethod: HomeAssistantMethod,
				effectTypes: this.SimpleItemList([EffectType.HttpGet, EffectType.HttpPost, EffectType.BroadLink, EffectType.iTach, EffectType.Vera, EffectType.HomeAssistant]),
				veraServices: this.SimpleItemList([VeraService.DimmerValue, VeraService.SwitchSet, VeraService.CurtainStop]),
				deleting: false
			};
		},
		computed:
		{
			broadlinkNameOptions()
			{
				let data = this.$store.getters.GetCachedResponse("broadlink_names");
				return this.SimpleItemList(data);
			},
			broadlinkCommandOptions()
			{
				let data = this.$store.getters.GetCachedResponse("broadlink_command_short_names");
				return this.SimpleItemList(data);
			},
			itachNameOptions()
			{
				let data = this.$store.getters.GetCachedResponse("itach_names");
				return this.SimpleItemList(data);
			},
			irCommandOptions()
			{
				let data = this.$store.getters.GetCachedResponse("ir_command_short_names");
				return this.SimpleItemList(data);
			},
			veraNameOptions()
			{
				let data = this.$store.getters.GetCachedResponse("vera_names");
				return this.SimpleItemList(data);
			},
			veraDeviceOptions()
			{
				let options = [];
				let data = this.$store.getters.GetCachedResponse("vera_command_list");
				if (data && this.effect && this.effect.data && this.effect.data.vera_name)
				{
					for (let v = 0; v < data.length; v++)
					{
						let vera = data[v];
						if (vera && vera.Name === this.effect.data.vera_name)
						{
							for (let i = 0; i < vera.DeviceIds.length; i++)
							{
								options.push({ Value: vera.DeviceIds[i], Text: vera.Names[i] });
							}
							break;
						}
					}
				}
				return options;
			},
			homeAssistantNameOptions()
			{
				let data = this.$store.getters.GetCachedResponse("hass_names");
				return this.SimpleItemList(data);
			},
			homeAssistantEntities()
			{
				let entities = [{ Value: "", Text: "" }];
				let data = this.$store.getters.GetCachedResponse("hass_entities");
				if (data && this.effect && this.effect.data && this.effect.data.hass_servername)
				{
					for (let i = 0; i < data.length; i++)
					{
						let ent = data[i];
						if (ent && ent.ServerName === this.effect.data.hass_servername)
						{
							let text = ent.FriendlyName + (ent.EntityId === ent.FriendlyName ? "" : (" (" + ent.EntityId + ")"));
							entities.push({ Value: ent.EntityId, Text: text });
						}
					}
				}
				return entities;
			},
			homeAssistantEffectRange()
			{
				let m = this.effect.data.hass_method;
				if (m)
				{
					if (m === HomeAssistantMethod.SwitchSet)
						return [0, 1];
					if (m === HomeAssistantMethod.DimmerValue)
						return [0, 255];
					if (m === HomeAssistantMethod.CoverSet)
						return [0, 100];
				}
				return null;
			},
			homeAssistantEffectRangeStr()
			{
				let r = this.homeAssistantEffectRange;
				if (r)
					return r[0] + " to " + r[1];
				return "";
			},
			homeAssistantMethods()
			{
				let e = this.effect.data.hass_entityid;
				if (e)
				{
					if (e.toLowerCase().indexOf("switch.") > -1)
						return this.SimpleItemList([HomeAssistantMethod.SwitchSet]);
					if (e.toLowerCase().indexOf("light.") > -1)
						return this.SimpleItemList([HomeAssistantMethod.DimmerValue, HomeAssistantMethod.SwitchSet]);
					if (e.toLowerCase().indexOf("cover.") > -1)
						return this.SimpleItemList([HomeAssistantMethod.CoverSet, HomeAssistantMethod.CoverStop]);
				}
				return this.SimpleItemList([]);
			}
		},
		methods:
		{
			edit()
			{
				this.$emit("edit", this.effect);
			},
			removeClick()
			{
				this.deleting = true;
				this.$emit("delete", this.effect);
			},
			resetState()
			{
				this.deleting = false;
			},
			editEffectType()
			{
				console.log(this.effect.type);
				if (this.effect.type === EffectType.Vera)
				{
					if (!this.effect.data.vera_service)
						this.effect.data.vera_service = this.VeraService.DimmerValue;
				}
				this.edit();
			},
			SimpleItemList(data)
			{
				let options = [];
				for (let i = 0; data && i < data.length; i++)
					options.push({ Value: data[i], Text: data[i] });
				return options;
			},
			validateHomeAssistantMethod()
			{
				if (this.homeAssistantMethods.length > 0)
				{
					for (let i = 0; i < this.homeAssistantMethods.length; i++)
						if (this.homeAssistantMethods[i].Value === this.effect.data.hass_method)
							return;
					this.effect.data.hass_method = this.homeAssistantMethods[0].Value;
				}
			}
		},
		created()
		{
			if (!this.effect.type)
			{
				this.effect.type = EffectType.HttpGet;
				this.effect.data = {};
			}
			this.validateHomeAssistantMethod();
		},
		mounted()
		{
		},
		beforeDestroy()
		{
		},
		watch:
		{
			effect: {
				deep: true,
				handler()
				{
					this.$nextTick(() =>
					{
						this.validateHomeAssistantMethod();
					});
				}
			}
		}
	};
</script>

<style scoped>
	.effectRoot
	{
		border: 1px solid rgba(0, 0, 0, 0.25);
		border-radius: 5px;
		background-color: #FFFFFF;
		padding: 5px;
		box-shadow: 1px 1px 3px rgba(0,0,0,0.25);
	}

	label
	{
		margin: 5px 0px;
	}

		label.wide
		{
			display: flex;
			align-items: baseline;
		}

			label.wide input
			{
				flex: 1 1 auto;
				margin-left: 4px;
			}

	.buttons
	{
		float: right;
		margin-left: 5px;
		margin-bottom: 5px;
	}

	svg
	{
		min-width: 40px;
		min-height: 40px;
		width: 40px;
		height: 40px;
		cursor: pointer;
		user-select: none;
		margin-left: 5px;
		border: 1px solid #CCCCCC;
		border-radius: 7px;
		background-color: rgba(0,0,0,0.1);
		fill: #000000;
		box-shadow: 1px 1px 3px rgba(0,0,0,0.5);
	}

		svg:hover
		{
			background-color: rgba(0,0,0,0.05);
		}

		svg:active
		{
			background-color: #FFFFFF;
		}

	.hotkeyEffect_dragHandle svg
	{
		transform: rotate(90deg);
		box-shadow: 1px -1px 3px rgba(0,0,0,0.5);
	}
</style>