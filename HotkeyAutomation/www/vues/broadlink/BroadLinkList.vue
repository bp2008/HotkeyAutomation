<template>
	<div>
		<List componentName="BroadLinkListItem" itemType="BroadLink RM" apiKey="broadlink"></List>
	</div>
</template>

<script>
	import List from 'appRoot/vues/common/List.vue';
	import { ExecJSON } from 'appRoot/api/api.js';

	export default {
		components: { List }
	};
</script>
<style scoped>
	.buttons
	{
		margin: 10px;
	}

	a
	{
		/*box-shadow: 1px 1px 3px rgba(0,0,0,0.5);*/
		/*background-color: rgba(0,123,255,0.05);*/ 
		border: 1px solid #007bff;
		padding: 2px 5px;
		color: #000000;
		-webkit-appearance: button;
	}
</style>