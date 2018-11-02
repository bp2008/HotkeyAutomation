<template>
	<div>
		<List componentName="VeraListItem" itemType="Vera" apiKey="vera"></List>
		<div class="buttons">
			<div>Reload the command lists after you are done adding or editing vera names or addresses.</div>
			<input type="button" value="Reload Command Lists" @click="reloadCommandList" />
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
				ExecJSON({ cmd: "vera_reload_commands" }).then(data =>
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