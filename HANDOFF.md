# Project handoff

## Rules for every human and AI contributor

1. Read this file and check the working tree before starting. Use the most recent entries to see what changed, what was verified, and what still needs attention.
2. Before **every** commit, add or finalize one entry that describes exactly what is going into that commit. Stage `HANDOFF.md` with the change and review the staged diff against the entry. If the staged scope changes, update the entry before committing.
3. Record who did the work, including the AI assistant or human contributor, who requested or approved it, and the actual Git author and committer if they differ. Use the name from Git for Git attribution. Keep AI assistance separate from Git authorship.
4. Record when the entry was prepared and the Git author date and time in ISO 8601 with a time zone offset. While work is pending, mark the Git author date and identity as pending. When the commit is authorized, choose the author timestamp before staging the final entry and use that same timestamp with `git commit --date`. Git records the committer timestamp and commit hash when it creates the commit. Use the commit subject and author timestamp to find the entry in Git.
5. Explain the purpose, changed files, technical behavior, decisions and rationale, and any meaningful tradeoffs. State what was verified, how it was verified, and the result. Note any remaining limitations or follow-up with enough detail for the next contributor to resume.
6. Preserve earlier entries. Add new entries above them. Keep passwords, tokens, private keys, and environment variable values out of the log.
7. Obtain explicit user authorization before committing or pushing. When authorization is pending, leave the entry marked **Pending** and the changes in the working tree.

## Entry template

```markdown
### Proposed or final commit subject

- Status: Pending / Ready for this commit
- Recorded at: YYYY-MM-DDTHH:MM:SS±HH:MM
- Work by: Human contributor(s) and AI assistant(s)
- Requested or approved by: Name or role and approved scope
- Git author: Configured name, or pending
- Git committer: Configured name if different, or pending
- Git author date: ISO 8601 with offset, or pending
- Purpose: What prompted the change and what the commit delivers
- Changes: Files, implementation details, and behavior
- Decisions: Choices, rationale, and meaningful tradeoffs
- Verification: Commands or manual steps, results, and gaps
- Follow-up: Remaining work, risks, or decisions
```

## Entries

### Document the pre-commit handoff workflow

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-11T10:55:12+01:00
- **Work by:** Codex
- **Requested by:** Project user, to make handoffs readable by humans and other AI assistants and mandatory before each commit.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-11T10:58:46+01:00
- **Changes:** `HANDOFF.md` records the rules, entry template, and pending project work. `AGENTS.md` directs coding assistants to the handoff before work and reinforces the existing authorization requirements.
- **Decisions:** Use one Markdown log with an entry in the same commit as the work it describes. Record the AI contributor separately from Git authorship. Use the commit subject and author timestamp to find the entry in history.
- **Verification:** Checked the rules and template for attribution, time zone, technical detail, verification results, and how to handle pending work.
- **Follow-up:** Use this workflow for the Docker environment commit and subsequent work.

### Set up the Babylon.js Docker environment

- **Status:** Pending
- **Recorded at:** 2026-11-11T10:55:12+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user requested Docker and explicitly approved both the development and presentation containers.
- **Git author, committer, and author date:** Pending until this work is committed.
- **Purpose:** Give both programmers a reproducible environment for the two-half-day browser prototype and a way to verify the compiled presentation build.
- **Changes:** `package.json`, `package-lock.json`, `tsconfig.json`, `index.html`, and `src/` define a TypeScript and Vite starter that renders a placeholder box with Babylon.js. `.nvmrc` and the Docker build use Node.js 24.22.2. `Dockerfile` installs locked dependencies with `npm ci`, runs Vite for development, and serves the compiled output in a separate unprivileged Nginx image. `compose.yaml` exposes development on localhost port 5173 and presentation on localhost port 8080. Compose Watch syncs edits and rebuilds on package file changes. `.dockerignore` excludes Git data, local dependencies, Blender source and backups, and local environment files. `README.md` and `TECHNICAL_DECISIONS.md` document both run modes.
- **Decisions:** Keep the two modes in one Dockerfile with separate targets. Keep development dependencies in the build and development stages and copy only the static Vite output into the presentation stage. Pin npm package versions and preserve the lockfile so both programmers install the same dependencies.
- **Verification:** `npm run build` and the Docker build passed. `npm audit` reported zero known dependency vulnerabilities. Chrome rendered the placeholder box from the compiled build. Both containers served the project on their localhost ports, and a temporary source edit reached the development server through Compose Watch. Verified on the local Apple Silicon Mac.
- **Follow-up:** The harbor import, gameplay, and minigames are still to be built. The second programmer still needs to run the setup on their machine after it is available on GitHub.
