
local nk = require("nakama")

local function create_match(context, payload)
    
    local matches = nk.match_list(10, true, "", 1, 1, nil)

    if #matches > 0 then
		nk.logger_info("Connecting to match")
        return nk.json_encode({ match_id = matches[1].match_id })
    else
		nk.logger_info("Creating an authoritative match")
        local match_id = nk.match_create("match_handler")
        return nk.json_encode({ match_id = match_id })
    end
end

nk.register_rpc(create_match, "create_match")

local M = {}
nk.logger_info("Module is loaded")


local TICK_RATE = 1 / 10 
local NPCSpawnInterval = 2

local function fillArray(enemyArray,enemyIdCounter)
	for i = 0,1,1 do
	local enemy = 
	{
        id = enemyIdCounter,
        position = { x = math.random(-4,4), y = -0.6, z = math.random(13,15) },
        type = math.random(0,1),
        health = 25
    }
	table.insert(enemyArray, enemy)
	nk.logger_info("New enemy id = " .. tostring(enemyIdCounter))
	enemyIdCounter = enemyIdCounter + 1
	end
end
local function enemyDead(enemyArray, id)

	if enemyArray[tonumber(id+1)] then
		table.remove(enemyArray, tonumber(id+1));
	end
end
local function enemyRequest_rpc(context, payload)
	nk.logger_info("Enemy Requested")
	return nk.json_encode(enemyArray)
end
local function enemyAdd_rpc(context, payload)
	nk.logger_info("Enemy Added")
	fillArray(enemyArray)
	return nk.json_encode(enemyArray)
end
local function enemyRemove_rpc(context, payload)
	nk.logger_info("Enemy Removed")
	enemyDead(enemyArray, payload)
	return nk.json_encode(enemyArray)
end

nk.register_rpc(enemyRequest_rpc, "enemyRequest_rpc")
nk.register_rpc(enemyAdd_rpc, "enemyAdd_rpc")
nk.register_rpc(enemyRemove_rpc, "enemyRemove_rpc")

function M.match_init(context, setupstate)
    local state = 
	{
		messages = {},
		lastTimeSpawned = 2,
		enemyIdCounter = 0,
		enemyArray = {},
		players = {}
	}
	 local tick_rate = 1
	 local label = ""
	nk.logger_info("InitCompleted")
    return state, tick_rate, label
end
function M.match_loop(context, dispatcher, tick, state, messages)
    state.lastTimeSpawned = state.lastTimeSpawned + TICK_RATE
    if state.lastTimeSpawned >= NPCSpawnInterval then
		state.lastTimeSpawned = 0
		fillArray(state.enemyArray, state.enemyIdCounter)

		local op_code = 1
		dispatcher.broadcast_message(op_code, nk.json_encode(state.enemyArray), nil, nil)
	end
        
	for _, message in ipairs(messages) do
		if (message.op_code == 0) then
			local decoded = nk.json_decode(message.data)
			local player = state.players[decoded.Id+1]
			if player then
				player.Position = decoded.Position
				player.Rotation = decoded.Rotation
				table.insert(state.messages, {
					Id = player.Id,
					Position = player.Position,
					Rotation = player.Rotation
				})
			end
			local op_code = 0
			if #state.messages > 0 then
				dispatcher.broadcast_message(op_code, nk.json_encode(state.messages), nil, nil)
				state.messages = {}
			end
		elseif(message.op_code == 2) then
			for i, enemy in ipairs(state.enemyArray) do
				if enemy.id == tonumber(message.data) then
					table.remove(state.enemyArray, i)
					break
				end
			end
				local op_code = 2
				dispatcher.broadcast_message(op_code, message.data, nil, nil)
				state.enemyIdCounter = #state.enemyArray

		end
	
	end
	
	return state
end

function M.match_join_attempt(context, dispatcher, tick, state, presence, metadata)
  local acceptuser = true
  return state, acceptuser
end

function M.match_signal(context, dispatcher, tick, state, data)
  return state, "signal received: " .. data
end

function M.match_join(context, dispatcher, tick, state, presences)
    nk.logger_info("Player joined the match")
	local newPlayer = 
	{
		Id = #state.players,
		Position = {X = 0, Y = 0, Z = 0},
		Rotation = {X = 0, Y = 0, Z = 0, W = 0}
	}
	table.insert(state.players, newPlayer)
	local op_code = 10
	dispatcher.broadcast_message(op_code, nk.json_encode(newPlayer), nil, nil)
	if (#state.enemyArray > 0)then
		local op_code = 1
		dispatcher.broadcast_message(op_code, nk.json_encode(enemyArray), nil, nil)
	end
    return state
end

function M.match_leave(context, dispatcher, tick, state, presences)
    nk.logger_info("Player left the match")
    return state
end

function M.match_terminate(context, dispatcher, tick, state, grace_seconds)
    nk.logger_info("Match terminated")
    return state
end

return M