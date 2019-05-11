<template>
	<div>
		<List componentName="BroadLinkCommandItem" itemType="BroadLink Command" apiKey="broadlinkcmd"></List>
		<div class="buttons">
			<input type="button" value="Reload BroadLink RM Commands" @click="reloadCommandList" title="Reloads commands from BroadLinkCommands.json" />
		</div>
	</div>
</template>

<script>
	import List from 'appRoot/vues/common/List.vue';
	import { ExecJSON } from 'appRoot/api/api.js';

	export default {
		components: { List },
		props:
		{
			controllerId: { // Currently unused by this component, but could be used to load the controller name into the UI
				type: String,
				required: true
			}
		},
		data()
		{
			return {
				isLoadingCommandList: false
			};
		},
		methods:
		{
			reloadCommandList()
			{
				if (this.isLoadingCommandList)
					return;
				this.isLoadingCommandList = true;
				ExecJSON({ cmd: "broadlinkcmd_reload_commands" }).then(data =>
				{
					toaster.success(data.data);
				}
				).catch(err =>
				{
					toaster.error(err);
				}
				).finally(() =>
				{
					this.isLoadingCommandList = false;
				});
			}
		},
		created()
		{
			this.$store.dispatch("CacheApiResponse", "broadlinkcmd_names");
		}
	};
</script>
<style scoped>
	.buttons
	{
		margin: 10px;
	}
</style>