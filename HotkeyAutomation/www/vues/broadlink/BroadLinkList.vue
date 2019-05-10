<template>
	<div>
		<List componentName="BroadLinkListItem" itemType="BroadLink RM" apiKey="broadlink"></List>
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
				ExecJSON({ cmd: "broadlink_reload_commands" }).then(data =>
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
		}
	};
</script>
<style scoped>
	.buttons
	{
		margin: 10px;
	}
</style>