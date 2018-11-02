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
			<div v-else-if="effect.type === EffectType.iTach" key="itach">
				<label>iTach Name: <VSelect v-model="effect.data.itach_name" :options="itachNameOptions" @change="edit" /></label>
				<label>Connector Address: <input type="text" v-model="effect.data.itach_connectorAddress" @change="edit" placeholder="e.g. 1:1 or 1:2 or 1:3" /></label>
				<label>Command Name: <VSelect v-model="effect.data.itach_commandShortName" :options="itachCommandOptions" @change="edit" /></label>
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
			<div v-else key="unknownEffectType">
				Unknown Effect Type: {{effect.type}}
			</div>
		</div>
	</div>
</template>

<script>
	import svg1 from 'appRoot/images/sprite/remove.svg';
	import VSelect from 'appRoot/vues/common/controls/VSelect.vue';
	import { EffectType, VeraService } from 'appRoot/scripts/EffectData.js';

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
				effectTypes: this.SimpleItemList([EffectType.HttpGet, EffectType.iTach, EffectType.Vera]),
				veraServices: this.SimpleItemList([VeraService.DimmerValue, VeraService.SwitchSet, VeraService.CurtainStop]),
				deleting: false
			};
		},
		computed:
		{
			itachNameOptions()
			{
				let data = this.$store.getters.GetCachedResponse("itach_names");
				return this.SimpleItemList(data);
			},
			itachCommandOptions()
			{
				let data = this.$store.getters.GetCachedResponse("itach_command_short_names");
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
				for (let i = 0; i < data.length; i++)
					options.push({ Value: data[i], Text: data[i] });
				return options;
			}
		},
		created()
		{
			if (!this.effect.type)
			{
				this.effect.type = EffectType.HttpGet;
				this.effect.data = {};
			}
		},
		mounted()
		{
		},
		beforeDestroy()
		{
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