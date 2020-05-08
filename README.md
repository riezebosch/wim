First download all work items UNDER specified area path's:
`wim clone --organization ... --token ... --area-path ... --area-path ... --output ...`

Then replay all updates:
`wim push --organization ... --token ... --project ... --output ...`

What it does:
* Download and replay all updates on selected work items in order!
* Recreate (and remove) links between work items in order
* Recreate areas & iterations on the fly!
* Upload and link to attachments!
* Handle more then 20k items with ease
* Retry and backoff when one hits the REST API rate limits
* Stop and continue at any time.

What it doesn't do yet:
* Recreate other type of links
* Recreate classification nodes (areas and iterations) on the fly

What it won't do:
* Migrate other things