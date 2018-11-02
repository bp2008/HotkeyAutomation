<template>
	<div>
		<List componentName="iTachListItem" itemType="iTach" apiKey="itach"></List>
		<div class="buttons">
			<input type="button" value="Reload Command List" @click="reloadCommandList" />
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
				ExecJSON({ cmd: "itach_reload_commands" }).then(data =>
				{
					toaster.success("Reloaded iTach command list from file");
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