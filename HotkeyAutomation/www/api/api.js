export function ExecJSON(args)
{
	if (!args.session)
		args.session = window.myApp.$store.getters.sid;
	var fetchArgs = {
		method: 'POST',
		headers: {
			'Accept': 'application/json',
			'Content-Type': 'application/json'
		},
		body: JSON.stringify(args),
		credentials: "same-origin"
	};
	let abort_controller = null;
	if (typeof AbortController === "function")
	{
		// FF 57+, Edge 16+ (in theory), Chrome 66+
		// Broken in Edge 17.x and 18.x (connection stays open)
		// Unknown when it will be fixed
		abort_controller = new AbortController();
		fetchArgs.signal = abort_controller.signal;
	}
	let fetcher = fetch(appContext.appPath + 'json', fetchArgs)
		.then(response => response.json())
		.then(data =>
		{
			if (data.success)
				return Promise.resolve(data);
			else
				return Promise.reject(new ApiError(data.error, data));
		}
		).catch(err =>
		{
			return Promise.reject(err);
		});
	fetcher.cancelRequest = () =>
	{
		if (abort_controller)
			abort_controller.abort();
	};
	return fetcher;
}
export class ApiError extends Error
{
	constructor(message, data)
	{
		super(message);
		this.name = "ApiError";
		this.data = data;
	}
}