<template>
	<div class="appRoot">
		<ModalDialogContainer name="dialogFade" transition-name="dialogFade" />
		<div id="layoutRoot">
			<router-view></router-view>
		</div>
	</div>
</template>

<script>
	import ModalDialogContainer from 'appRoot/vues/common/dialogs/ModalDialogContainer.vue';

	export default {
		components: { ModalDialogContainer },
		data()
		{
			return {
			};
		},
		computed:
		{
		},
		methods:
		{
		},
		created()
		{
		}
	};
</script>

<style>
	/* UNSCOPED */
	.dialogFade-enter-active, .dialogFade-leave-active
	{
		transition: opacity .2s;
	}

	.dialogFade-enter, .dialogFade-leave-to
	{
		opacity: 0;
	}
</style>
<style scoped>
	.appRoot
	{
		font-size: 0px;
		height: 100%;
		width: 100%;
	}
</style>