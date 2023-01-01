<template>
	<div :class="{ itemRoot: true, deleting: deleting, working: working, isHotkey: isHotkey }">
		<div class="topRow">
			<input class="name" type="text" v-model="item.name" :placeholder="('Name this ' + itemType + '…')" @change="edit" />
			<label v-if="!isHotkey && !isBroadlinkCmd && !isHomeAssistantServer">
				Host:
				<input class="host" type="text" v-model="item.host" placeholder="192.168.x.x" @change="edit" />
			</label>
			<label v-if="!isHotkey && !isBroadlinkCmd && !isHomeAssistantServer && includePort">
				Port:
				<input class="port" type="number" v-model.number="item.port" min="1" max="65535" :placeholder="portPlaceholder" @change="edit" />
			</label>
			<label v-if="isHomeAssistantServer">
				URL:
				<input class="url" type="text" v-model="item.url" placeholder="http://homeassistant:8123/" @change="edit" />
			</label>
			<label v-if="isHomeAssistantServer">
				API KEY:
				<input class="apiKey" type="text" v-model="item.apiKey" placeholder="01234567890abcdef" @change="edit" />
			</label>
			<span v-if="isHotkey" title="Bind new key"><svg @click="hotkeyListen"><use xlink:href="#input"></use></svg></span>
			<span v-if="isHotkey" class="keyReadout">{{item.keyName}}</span>
			<span v-if="isBroadlinkCmd" title="Learn new codes"><svg @click="$emit('learnCodes')"><use xlink:href="#input"></use></svg></span>
			<span v-if="isBroadlinkCmd" class="keyReadout">{{item.codes ? "Codes Learned" : ""}}</span>
			<router-link v-if="isBroadlinkController" :to="{ name: 'broadlinkcmds', params: { controllerId: this.item.id.toString() } }">Manage Commands</router-link>
			<div class="buttons">
				<label v-if="isHotkey" title="If checked, the hotkey will only activate if pressed twice in a short time period."><input type="checkbox" v-model="item.doublePress"  @change="edit" /> Dbl</label>
				<ScaleLoader :class="{ scaleLoader: true, visible: working }"></ScaleLoader>
				<span title="Drag me!" class="listItem_dragHandle"><svg><use xlink:href="#arrows"></use></svg></span>
				<span :title="('Delete this ' + itemType)"><svg @click="removeClick"><use xlink:href="#remove"></use></svg></span>
			</div>
		</div>
		<div v-if="isHotkey">
			<draggable v-model="item.effects" @change="itemsChanged" :options="{ handle: '.hotkeyEffect_dragHandle' }">
				<HotkeyEffect v-for="(effect, index) in item.effects" :key="index" :effect="effect" class="hotkeyEffect" @edit="edit" @delete="deleteEffect" />
			</draggable>
		</div>
		<div v-if="isBroadlinkCmd">{{item.codes}}</div>
		<div v-if="isBroadlinkCmd">
			<span>Type: <b>{{item.type}}</b></span>
			&nbsp; &nbsp;
			<label>
				Default Repeat Count (0-255):
				<input type="number" v-model.number="item.repeat" min="0" max="255" placeholder="Repeat" @change="edit" />
			</label>
		</div>
		<div class="bottomButtons">
			<input v-if="isHotkey" type="button" value="Add Effect" @click="addEffect" class="addEffect" />
			<input v-if="isHotkey" type="button" value="Execute Hotkey Effects" @click="executeHotkey" class="executeHotkey" />
		</div>
	</div>
</template>

<script>
	import svg1 from 'appRoot/images/sprite/remove.svg';
	import svg2 from 'appRoot/images/sprite/input.svg';
	import svg3 from 'appRoot/images/sprite/arrows.svg';
	import { ModalHotkeyListener } from 'appRoot/scripts/Dialog.js';
	import HotkeyEffect from 'appRoot/vues/hotkeys/HotkeyEffect.vue';
	import { Effect } from 'appRoot/scripts/EffectData.js';
	import draggable from 'vuedraggable';

	export default {
		components: { HotkeyEffect, draggable },
		props:
		{
			item: {
				type: Object,
				required: true
			},
			itemType: {
				type: String,
				required: true
			},
			apiKey: {
				type: String,
				required: true
			},
			includePort: {
				type: Boolean,
				default: false
			},
			portPlaceholder: {
				type: String,
				default: "Port"
			}
		},
		data()
		{
			return {
				working: false,
				deleting: false
			};
		},
		computed:
		{
			isHotkey()
			{
				return this.apiKey === "hotkey";
			},
			isBroadlinkCmd()
			{
				return this.apiKey === "broadlinkcmd";
			},
			isBroadlinkController()
			{
				return this.apiKey === "broadlink";
			},
			isHomeAssistantServer()
			{
				return this.itemType === "HomeAssistant";
			},
			undoItems()
			{
				let component = this;
				while (component.$parent)
				{
					component = component.$parent;
					if (component.undoItems)
						return component.undoItems;
				}
				toaster.error("Could not find a list of undo items in the ancestor components");
				return [];
			}
		},
		methods:
		{
			hotkeyListen()
			{
				ModalHotkeyListener(this.item.id).then(result =>
				{
					if (result)
					{
						this.item.key = result.key;
						this.item.keyName = result.keyName;
					}
				});
			},
			removeClick()
			{
				this.working = true;
				this.deleting = true;
				this.$emit("delete", this.item);
			},
			resetState()
			{
				this.working = false;
				this.deleting = false;
			},
			edit()
			{
				this.working = true;
				this.$emit("edit", this.item);
			},
			addEffect()
			{
				if (!this.item.effects)
					this.item.effects = [];
				this.item.effects.push(new Effect());
				this.edit();
			},
			executeHotkey()
			{
				this.working = true;
				this.$emit("executeHotkey", this.item);
			},
			deleteEffect(effect)
			{
				let idxKey = this.item.effects.indexOf(effect);
				if (idxKey > -1)
					this.item.effects.splice(idxKey, 1);
				this.edit();
				this.setupUndoDelete(effect);
			},
			setupUndoDelete(effect)
			{
				let undoItem = {};
				undoItem.deletedItem = effect;
				undoItem.description = "Deleted effect";
				undoItem.undo = () =>
				{
					this.undoDelete(undoItem);
				};
				undoItem.expire = 6;
				undoItem.expireTimeout = setTimeout(() =>
				{
					this.expireUndoItem(undoItem);
				}, 6000);
				this.undoItems.push(undoItem);
			},
			undoDelete(undoItem)
			{
				undoItem.expire = 0;
				clearTimeout(undoItem.expireTimeout);
				undoItem.expireTimeout = null;
				this.expireUndoItem(undoItem);
				this.item.effects.push(undoItem.deletedItem);
				this.edit();
			},
			expireUndoItem(undoItem)
			{
				clearTimeout(undoItem.expireTimeout);
				let idxUndoItem = this.undoItems.indexOf(undoItem);
				if (idxUndoItem > -1)
					this.undoItems.splice(idxUndoItem, 1);
			},
			itemsChanged(arg)
			{
				if (arg.moved)
				{
					this.edit();
				}
			}
		},
		created()
		{
		}
	};
</script>

<style scoped>
	.itemRoot
	{
		border-bottom: 1px solid #bfbfbf;
		padding: 1px 5px;
		overflow: hidden;
	}

		.itemRoot:last-child
		{
			border-bottom: none;
		}

		.itemRoot.deleting
		{
			filter: blur(2px);
		}

		.itemRoot.working
		{
			background-color: rgba(0,0,0,0.05);
		}

		.itemRoot.isHotkey
		{
			padding-top: 8px;
			padding-bottom: 8px;
			background-color: rgba(0,0,0,0.05);
		}

			.itemRoot.isHotkey:last-child
			{
				padding-bottom: 1px;
				margin-bottom: 0px;
			}

			.itemRoot.isHotkey:first-child
			{
				padding-top: 1px;
			}

			.itemRoot.isHotkey.working
			{
				background-color: rgba(0,0,0,0.1);
			}

	.topRow
	{
		display: flex;
		flex-wrap: wrap;
		align-items: center;
		justify-content: space-between;
	}

	.name
	{
		width: 250px;
		padding-left: 3px;
		margin: 5px 0px;
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

	label
	{
		margin-bottom: 0px; /* Bootstrap sure is opinionated */
		margin-left: 5px;
	}

	.keyReadout
	{
		flex: 1 1 auto;
		margin-left: 9px;
	}

	.buttons
	{
		display: flex;
		align-items: center;
	}

	.scaleLoader
	{
		opacity: 0;
	}

		.scaleLoader.visible
		{
			opacity: 1;
		}

	.bottomButtons
	{
		margin-top: 5px;
	}

	.hotkeyEffect,
	.addEffect
	{
		margin: 3px 0px 10px 0px;
	}

	.executeHotkey
	{
		float: right;
	}

	.listItem_dragHandle svg
	{
		transform: rotate(90deg);
		box-shadow: 1px -1px 3px rgba(0,0,0,0.5);
	}
</style>