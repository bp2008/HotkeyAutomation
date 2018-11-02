<template>
	<select :value="myValue" @input="$emit('input', $event.target.value)" @change="$emit('change')">
		<option v-for="opt in options" :value="opt.Value">{{opt.Text}}</option>
	</select>
</template>

<script>

	export default {
		props:
		{
			value: {
				required: true
			},
			options: {
				type: Array,
				default: null
			}
		},
		data()
		{
			return {
				myValue: this.value
			};
		},
		watch:
		{
			value()
			{
				this.myValue = this.value;
			},
			options()
			{
				let tmpVal = this.value;
				this.myValue = null;
				this.myValue = tmpVal;
			}
		}
	};
</script>
<style scoped>
	select
	{
		max-width: 300px;
	}
</style>