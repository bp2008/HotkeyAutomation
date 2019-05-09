<template>
	<div :class="{ refreshingLog: true, error: error }" ref="refreshingLog">
	</div>
</template>

<script>
	import { ExecJSON } from 'appRoot/api/api.js';

	export default {
		components: {},
		data()
		{
			return {
				nextLine: -1,
				logId: -1,
				error: false,
				isLogScrolledToBottom: true,
				isClosing: false,
				cancelLogRefresh: () => { }
			};
		},
		methods:
		{
			Refresh()
			{
				if (this.isClosing)
					return;
				let apiPromise = ExecJSON({ cmd: "log_get", nextLine: this.nextLine, logId: this.logId });
				this.cancelLogRefresh = apiPromise.cancelRequest;
				apiPromise.then(data =>
				{
					if (data.status === "OK")
					{
						this.error = false;
						this.logId = data.logId;
						this.nextLine = data.nextLine;
						if (data.lines && data.lines.length > 0)
						{
							this.SaveLogScroll();
							for (let i = 0; i < data.lines.length; i++)
								this.AddLine(data.lines[i]);
							this.LoadLogScroll();
						}
						this.Refresh();
					}
					else if (data.status === "REFRESH")
					{
						// When the server restarts, it creates a new logId, which triggers this refresh
						this.logId = data.logId;
						this.nextLine = data.nextLine;
						this.$refs.refreshingLog.innerHTML = "";
						this.Refresh();
					}
					else
					{
						this.error = true;
						toaster.error("Unknown log status. Log updating halted.", data.status, null, 300000);
					}
				}
				).catch(err =>
				{
					this.error = true;
					if (err.code !== DOMException.ABORT_ERR)
						toaster.error("Error retrieving log data (retry in 10s)", err, 30000);
					if (!this.isClosing)
						setTimeout(() =>
						{
							this.Refresh();
						}, 10000);
				});
			},
			AddLine(line)
			{
				if (!this.$refs.refreshingLog)
					return;
				let ele = document.createElement("div");
				ele.innerText = line;
				this.$refs.refreshingLog.appendChild(ele);
			},
			SaveLogScroll()
			{
				if (!this.$refs.refreshingLog)
					return;
				let scrollParent = Util.GetScrollParent(this.$refs.refreshingLog);
				this.isLogScrolledToBottom = scrollParent.scrollTop + scrollParent.clientHeight + 200 >= this.$refs.refreshingLog.scrollHeight;
			},
			LoadLogScroll()
			{
				if (!this.$refs.refreshingLog)
					return;
				if (this.isLogScrolledToBottom)
					Util.GetScrollParent(this.$refs.refreshingLog).scrollTop = this.$refs.refreshingLog.scrollHeight;
			}
		},
		mounted()
		{
			this.Refresh();
		},
		beforeDestroy()
		{
			this.isClosing = true;
			this.cancelLogRefresh();
		}
	};
	function SetParentScrollTop(ele, h)
	{
		console.log(ele, h);
		Util.GetScrollParent(ele).scrollTop = h;
	}
</script>

<style>
	.refreshingLog > div:nth-child(even)
	{
		background-color: rgba(0,0,0,0.06);
	}
</style>
<style scoped>
	.refreshingLog
	{
		min-height: 3px; /* Just enough minimum height to ensure the background color is easily noticeable in the event of an error */
		word-break: break-word;
	}

		.refreshingLog.error
		{
			background-color: #ffd0d0;
		}
</style>