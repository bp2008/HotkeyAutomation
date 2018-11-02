<template>
	<div class="dialogRoot" @click="readyToClose">
		<div class="dialogContent" v-on:click="stopProp">
			<div :class="{ status:true, error: error }">{{status}}</div>
			<div v-if="!error" class="unbind" @click="unbind"><span title="Removes the key binding from this hotkey">Unbind</span></div>
			<div class="hint">Click outside this box to cancel.</div>
		</div>
	</div>
</template>

<script>
	import { ExecJSON } from 'appRoot/api/api.js';

	export default {
		components: {},
		props:
		{
			hotkeyId: {
				type: Number,
				required: true
			}
		},
		data()
		{
			return {
				status: "Loading…",
				error: false,
				bindId: null
			};
		},
		computed:
		{
		},
		methods:
		{
			stopProp(event)
			{
				event.stopPropagation();
			},
			readyToClose()
			{
				if (this.bindId !== null)
				{
					this.status = "Canceling…";
					ExecJSON({ cmd: "cancelHotkeyBind", bindId: this.bindId }).finally(() =>
					{
						this.$close(false);
					});
				}
				else
					this.$close(false);
			},
			unbind()
			{
				ExecJSON({ cmd: "unbindHotkey", hotkeyId: this.hotkeyId, bindId: this.bindId }).then(data =>
				{
					this.$close(data.data);
				}
				).catch(err =>
				{
					console.error(err);
					this.$close(false);
				});
			}
		},
		created()
		{
			ExecJSON({ cmd: "beginHotkeyBind", hotkeyId: this.hotkeyId }).then(data =>
			{
				this.bindId = data.data;
				this.status = "Press a key on the server's keyboard/keypad now";

				ExecJSON({ cmd: "endHotkeyBind", hotkeyId: this.hotkeyId, bindId: this.bindId }).then(data =>
				{
					this.$close(data.data);
				}
				).catch(err =>
				{
					this.error = true;
					this.status = err.message;
					console.error(err);
				}
				).finally(() =>
				{
					this.bindId = null;
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