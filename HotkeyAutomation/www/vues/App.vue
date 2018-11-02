<template>
	<div class="appRoot">
		<dialogs-wrapper name="dialogDefault"></dialogs-wrapper>
		<dialogs-wrapper name="dialogFade" transition-name="dialogFade"></dialogs-wrapper>
		<router-view></router-view>
	</div>
</template>

<script>

	export default {
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

	.dialogRoot
	{
		position: fixed;
		z-index: 10000;
		top: 0px;
		left: 0px;
		width: 100%;
		height: 100%;
		background-color: rgba(0,0,0,0.15);
		display: flex;
		align-items: center;
		justify-content: center;
	}

	.dialogContent
	{
		max-width: 90%;
		max-height: 90%;
		border: 1px solid #888888;
		border-radius: 7px;
		box-shadow: 0px 0px 3px rgba(0,0,0,0.5);
		background-color: #FFFFFF;
		font-size: 16px;
		padding: 5px;
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