<script>
	import { store } from "./TodoMVC.fs.js"

	const updateView = () => {
		let visibility = 'all';
		if (window.location.hash === '#/active') {
			visibility = 'active';
		} else if (window.location.hash === '#/completed') {
			visibility = 'completed';
		}
		store.on("ChangeVisibility", visibility);
	};

	window.addEventListener('hashchange', updateView);
	updateView();

	$: filtered = $store.visibility === 'all'
		? $store.entries
		: $store.visibility === 'completed'
			? $store.entries.filter(item => item.completed)
			: $store.entries.filter(item => !item.completed);

	$: numActive = $store.entries.filter(item => !item.completed).length;

	$: numCompleted = $store.entries.filter(item => item.completed).length;
</script>

<header class="header">
	<h1>todos</h1>
	<input
		class="new-todo"
		on:keydown={ev => {
			if (ev.key === "Enter") {
				store.on("Add", ev.target.value)
				ev.target.value = '';
			}
		}}
		placeholder="What needs to be done?"
		autofocus
	>
</header>

{#if $store.entries.length > 0}
	<section class="main">
		<input id="toggle-all" class="toggle-all" type="checkbox" on:change={() => store.on("CheckAll")} checked="{numCompleted === $store.entries.length}">
		<label for="toggle-all">Mark all as complete</label>

		<ul class="todo-list">
			{#each filtered as item}
				<li class="{item.completed ? 'completed' : ''} {item.editing ? 'editing' : ''}">
					<div class="view">
						<input class="toggle" type="checkbox" bind:checked={item.completed}>
						<label on:dblclick="{() => store.on("Edit", item.id, true)}">{item.description}</label>
						<button on:click="{() => store.on("Delete", item.id)}" class="destroy"></button>
					</div>

					{#if item.editing}
						<input
							value='{item.description}'
							id="edit"
							class="edit"
							on:keydown={ev => {
								if (ev.key === "Enter") {
									store.on("Update", item.id, ev.target.value)
								} else if (ev.key === "Escape") {
									ev.target.blur();
								}
							}}
							on:blur={_ => store.on("Edit", item.id, false)}
							autofocus
						>
					{/if}
				</li>
			{/each}
		</ul>

		<footer class="footer">
			<span class="todo-count">
				<strong>{numActive}</strong> {numActive === 1 ? 'item' : 'items'} left
			</span>

			<ul class="filters">
				<li><a class="{$store.visibility === 'all' ? 'selected' : ''}" href="#/">All</a></li>
				<li><a class="{$store.visibility === 'active' ? 'selected' : ''}" href="#/active">Active</a></li>
				<li><a class="{$store.visibility === 'completed' ? 'selected' : ''}" href="#/completed">Completed</a></li>
			</ul>

			{#if numCompleted}
				<button class="clear-completed" on:click={() => store.on("DeleteComplete")}>
					Clear completed
				</button>
			{/if}
		</footer>
	</section>
{/if}