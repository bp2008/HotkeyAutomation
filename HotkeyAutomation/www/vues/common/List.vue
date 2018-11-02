<template>
	<div class="listRoot">
		<div v-if="(error)" class="error">{{error}}</div>
		<div v-else-if="loading" class="loading"><ScaleLoader /></div>
		<div v-else>
			<div class="buttons">
				<div :title="('Add a new ' + itemType)" @click="newItem" tabindex="0"><svg><use xlink:href="#add"></use></svg> Add a new {{itemType}}</div>
			</div>
			<div ref="itemList" class="itemList" v-if="items.length > 0">
				<draggable v-model="items" @change="itemsChanged" :options="{ handle: '.listItem_dragHandle' }">
					<component :is="componentName" class="item" v-for="item in items" :key="item[idField]" :item="item" @delete="deleteItem" @edit="editItem" ref="listItems" />
				</draggable>
			</div>
			<UndoStack :items="undoItems" />
		</div>
	</div>
</template>

<script>
	import { ExecJSON } from 'appRoot/api/api.js';
	import svg1 from 'appRoot/images/sprite/add.svg';
	import UndoStack from 'appRoot/vues/common/undo/UndoStack.vue';
	import Vue from 'vue';
	import draggable from 'vuedraggable';

	export default {
		components: { UndoStack, draggable },
		data()
		{
			return {
				error: null,
				loading: false,
				items: {
					type: Array,
					required: true
				},
				undoItems: []
			};
		},
		props:
		{
			componentName: {
				type: String,
				required: true // Component names must be registered globally in main.js
			},
			itemType: {
				type: String,
				required: true
			},
			apiKey: {
				type: String,
				required: true
			},
			idField: {
				type: String,
				default: "id"
			}
		},
		methods:
		{
			loadList()
			{
				this.loading = true;
				ExecJSON({ cmd: this.apiKey + "_list" }).then(data =>
				{
					this.items = data.data;
				}
				).catch(err =>
				{
					this.error = err.message;
				}
				).finally(() =>
				{
					this.loading = false;
				});
			},
			newItem()
			{
				ExecJSON({ cmd: this.apiKey + "_new" }).then(data =>
				{
					this.items.push(data.data);
				}
				).catch(err =>
				{
					toaster.error(err);
				}
				).finally(() =>
				{
					this.working = false;
				});
			},
			deleteItem(item)
			{
				ExecJSON({ cmd: this.apiKey + "_delete", id: item[this.idField] }).then(data =>
				{
					let idxKey = this.items.indexOf(item);
					if (idxKey > -1)
						this.items.splice(idxKey, 1);
					this.setupUndoDelete(item);
				}
				).catch(err =>
				{
					toaster.error(err);
					this.resetItemState(item);
				});
			},
			editItem(item)
			{
				ExecJSON({ cmd: this.apiKey + "_update", item: item }).then(data =>
				{
					this.resetItemState(item);
				}
				).catch(err =>
				{
					this.resetItemState(item);
					toaster.error(err);
					if (err.name === "ApiError" && err.data)
					{
						let idxKey = this.items.indexOf(item);
						if (idxKey > -1)
							Vue.set(this.items, idxKey, err.data.data);
					}
				});
			},
			setupUndoDelete(item)
			{
				let undoItem = {};
				undoItem.deletedItem = item;
				undoItem.description = "Deleted " + item.name;
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
				// We must re-create this item. It will probably receive a new ID.
				ExecJSON({ cmd: this.apiKey + "_new" }).then(data =>
				{
					undoItem.deletedItem[this.idField] = data.data[this.idField];
					// Update the new item to match the delete one.
					ExecJSON({ cmd: this.apiKey + "_update", item: undoItem.deletedItem }).then(data =>
					{
						this.expireUndoItem(undoItem);
						this.items.push(undoItem.deletedItem);
					}
					).catch(err =>
					{
						toaster.error(err);
					});
				}
				).catch(err =>
				{
					toaster.error(err);
				});
			},
			expireUndoItem(undoItem)
			{
				clearTimeout(undoItem.expireTimeout);
				let idxUndoItem = this.undoItems.indexOf(undoItem);
				if (idxUndoItem > -1)
					this.undoItems.splice(idxUndoItem, 1);
			},
			resetItemState(item)
			{
				if (this.$refs.listItems)
				{
					for (let i = 0; i < this.$refs.listItems.length; i++)
					{
						let ele = this.$refs.listItems[i];
						if (ele.item === item)
						{
							if (ele && ele.resetState)
								ele.resetState();
							return;
						}
					}
				}
			},
			itemsChanged(arg)
			{
				if (arg.moved)
				{
					let ids = this.items.map(item => item.id);
					ExecJSON({ cmd: this.apiKey + "_reorder", ids: ids }).then(data =>
					{
						toaster.success(null, "Reordered " + this.itemType + " list", 1250);
					}
					).catch(err =>
					{
						toaster.error(err);
					});
				}
			}
		},
		created()
		{
			this.loadList();
		}
	};
</script>

<style scoped>
	.listRoot
	{
		margin: 8px;
	}

	.loading
	{
		margin-top: 80px;
		text-align: center;
	}

	.error
	{
		color: #FF0000;
		font-weight: bold;
	}

	.itemList
	{
		border: 1px solid #888888;
		border-radius: 7px;
		max-width: 1200px;
	}

	.buttons
	{
		margin: 10px;
	}

		.buttons > *
		{
			display: inline-block;
			cursor: pointer;
			font-size: 20px;
			user-select: none;
			padding: 5px;
		}

			.buttons > *:hover
			{
				background-color: rgba(0,0,0,0.1);
			}

			.buttons > *:active
			{
				background-color: rgba(0,0,0,0.2);
			}

		.buttons svg
		{
			width: 48px;
			height: 48px;
		}
</style>