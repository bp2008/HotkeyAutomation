<template>
	<div>
		<div :class="{ status:true, error: error }">{{status}}</div>
		<div v-if="!error" class="unbind" @click="unbind"><span title="Removes the codes from this command">Unbind</span></div>
		<div class="hint">Click outside this box to cancel.</div>
	</div>
</template>

<script>
	import { ExecJSON } from 'appRoot/api/api.js';

	export default {
		components: {},
		props:
		{
			controllerId: {
				type: Number,
				required: true
			},
			commandId: {
				type: Number,
				required: true
			}
		},
		data()
		{
			return {
				status: "Loading…",
				error: false,
				lessonId: null
			};
		},
		computed:
		{
		},
		methods:
		{
			DefaultClose()
			{
				// This does not currently allow cancelation. If it ever needs to, we can copy code from HotkeyListener.vue.
				if (this.lessonId !== null)
				{
					this.status = "Canceling…";
					ExecJSON({ cmd: "cancelBroadlinkCommandLearn", controllerId: this.controllerId, lessonId: this.lessonId }).finally(() =>
					{
						this.$emit("close");
					});
				}
				else
					this.$emit("close");
			},
			unbind()
			{
				ExecJSON({ cmd: "unlearnBroadlinkCommand", controllerId: this.controllerId, lessonId: this.lessonId }).then(data =>
				{
					this.$emit("close", data.data);
				}
				).catch(err =>
				{
					console.error(err);
					this.$emit("close");
				});
			}
		},
		created()
		{
			ExecJSON({ cmd: "beginBroadlinkCommandLearn", controllerId: this.controllerId, commandId: this.commandId }).then(data =>
			{
				this.lessonId = data.data;
				this.status = "Teach the RM device a remote control button now";

				ExecJSON({ cmd: "endBroadlinkCommandLearn", controllerId: this.controllerId, commandId: this.commandId, lessonId: this.lessonId }).then(data =>
				{
					this.$emit("close", data.data);
				}
				).catch(err =>
				{
					this.error = true;
					this.status = err.message;
					console.error(err);
				}
				).finally(() =>
				{
					this.lessonId = null;
				});
			}
			).catch(err =>
			{
				this.error = true;
				this.status = err.message;
				console.error(err);
			});
		},
		mounted()
		{
		},
		beforeDestroy()
		{
		}
	};
</script>

<style scoped>
	.dialogContent
	{
		cursor: default;
	}

	.status
	{
		text-align: center;
		margin: 10px;
		border: 1px dotted black;
		background-color: #EEEEEE;
		padding: 15px;
	}

		.status.error
		{
			background-color: #AA0000;
			color: #FFFFFF;
		}

	.unbind
	{
		text-align: center;
		margin: 10px;
	}

		.unbind span
		{
			background-color: #ff8000;
			color: #FFFFFF;
			padding: 3px 6px;
			cursor: pointer;
		}

			.unbind span:hover
			{
				background-color: #ffae5c;
			}

	.hint
	{
		text-align: center;
		color: #888888;
		font-style: italic;
	}
</style>