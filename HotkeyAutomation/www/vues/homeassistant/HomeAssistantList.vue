<template>
	<div>
		<List componentName="HomeAssistantListItem" itemType="HomeAssistant" apiKey="hass"></List>
		<div class="buttons">
			<div>Reload the command lists after you are done adding or editing HomeAssistant server information.</div>
			<input type="button" value="Reload Entity Lists" @click="reloadEntityLists" />
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
				isLoadingEntityLists: false
			};
		},
		methods:
		{
			reloadEntityLists()
			{
				if (this.isLoadingEntityLists)
					return;
				this.isLoadingEntityLists = true;
				ExecJSON({ cmd: "hass_load" }).then(data =>
				{
					toaster.success(data.data);
				}
				).catch(err =>
				{
					toaster.error(err);
				}
				).finally(() =>
				{
					this.isLoadingEntityLists = false;
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