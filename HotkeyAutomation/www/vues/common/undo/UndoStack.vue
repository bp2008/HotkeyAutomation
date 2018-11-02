<template>
	<div class="undoStack" v-if="items.length > 0">
		<div class="undoItem" v-for="item in items" @click="item.undo">
			<div>{{item.description}}</div>
			<div>
				Click to UNDO
				<span v-if="item.expire > 0">(<Countdown :from="item.expire" />)</span>
			</div>
		</div>
	</div>
</template>

<script>
	import Countdown from 'appRoot/vues/common/controls/Countdown.vue';
	export default {
		components: { Countdown },
		props:
		{
			items: {
				type: Array,
				required: true
			}
		}
	};
</script>

<style scoped>
	.undoStack
	{
		position: fixed;
		bottom: 20px;
		right: 20px;
	}

	.undoItem
	{
		margin: 7px;
		border: 2px solid #FF8800;
		background-color: rgba(255,127,0,0.05);
		padding: 7px;
		cursor: pointer;
	}

		.undoItem:hover
		{
			background-color: rgba(0,0,0,0.015);
		}
</style>