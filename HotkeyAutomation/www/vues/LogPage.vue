<template>
	<div class="logPageRoot">
		<RefreshingLog class="refreshingLog" />
	</div>
</template>

<script>
	import RefreshingLog from 'appRoot/vues/common/RefreshingLog.vue';

	export default {
		components: { RefreshingLog }
	};
</script>
<style scoped>
	.logPageRoot
	{
		margin: 10px;
	}
</style>