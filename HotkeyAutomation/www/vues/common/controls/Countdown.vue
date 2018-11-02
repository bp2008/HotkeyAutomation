<template>
	<span>{{timerValue}}</span>
</template>

<script>

	export default {
		components: {},
		props:
		{
			from: {
				type: Number,
				required: true
			}
		},
		data()
		{
			return {
				timerValue: this.from,
				tickInterval: null
			};
		},
		created()
		{
			this.tickInterval = setInterval(() =>
			{
				this.timerValue -= 1;
				if (this.timerValue <= 0)
				{
					this.timerValue = 0;
					clearInterval(this.tickInterval);
				}
			}, 1000);
		},
		beforeDestroy()
		{
			clearInterval(this.tickInterval);
		}
	};
</script>