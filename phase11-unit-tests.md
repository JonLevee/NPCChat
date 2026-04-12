# Phase 11 — Unit Test Coverage

## Goal

Fill out comprehensive unit tests across NPCChat.Core and NPCChat.Editor.
**Core tests are highest priority** — the Core library will eventually run under Unity, so tests must cover the pure logic that travels with it. Editor tests cover internal tooling only and are lower priority.

Current state: 18 test methods total. AStarPathfinder is well-covered (13 tests); everything else is essentially untested.

---

## Core Tests (NPCChat.Tests)

Organized by priority. Each group maps to a new test file.

---

### Group 1 — Dialogue System (highest priority, most complex logic)

**`DialoguePickerTests.cs`**  
DialoguePicker is stateless and testable with a seeded `Random`. Most critical logic in the codebase.

- `PickOne_AllEntriesFiltered_ReturnsNull`
- `PickOne_SingleEligibleEntry_PicksIt`
- `PickOne_MoodAffinity_BelowMinThreshold_Filtered`
- `PickOne_MoodAffinity_AboveMin_PassesThrough`
- `PickOne_HardGate_RequiresGate_BlocksWhenGateNotMet`
- `PickOne_HardGate_ForbidsGate_BlocksWhenGateMatches`
- `PickOne_Intent_FiltersEntriesByIntent`
- `PickOne_Intent_NullIntentAcceptsAll`
- `PickOne_SelfCooldown_BlocksEntryDuringCooldown`
- `PickOne_SelfCooldown_AllowsEntryAfterCooldownExpires`
- `PickOne_GroupCooldown_BlocksAllEntriesInGroup`
- `PickOne_WeightedRandom_SeededRng_Deterministic`
- `PickOne_WeightedRandom_HigherWeightPickedMoreOften`
- `PickOne_NoAffinityPassEntries_FallsBackToWeightOnly`

**`CooldownTrackerTests.cs`**

- `IsOffCooldown_NoEntry_ReturnsTrue`
- `IsOffCooldown_SelfCooldown_NotYetExpired_ReturnsFalse`
- `IsOffCooldown_SelfCooldown_Expired_ReturnsTrue`
- `IsOffCooldown_GroupCooldown_NotYetExpired_ReturnsFalse`
- `IsOffCooldown_GroupCooldown_Expired_ReturnsTrue`
- `MarkUsed_SelfCooldown_RecordsTimestamp`
- `MarkUsed_GroupCooldown_RecordsTimestamp`
- `MultipleEntries_SameGroup_AllBlockedByGroupCooldown`

**`DialogueTreeBuilderTests.cs`**

- `AddLine_CreatesLineNodeWithText`
- `AddNpcLine_SetsSpeakerToNpc`
- `AddChoice_CreatesChoiceNodeWithAllChoices`
- `AddPool_CreatesPoolNodeWithEntries`
- `AddSequence_CreatesSequenceNodeWithChildren`
- `Build_AssignsUniqueIdToEachPoolEntry`
- `Build_NormalizesPoolEntryMoodVectors`
- `Build_AllNodesAccessibleFromRoot`
- `MoodAxes_SetOnBuilder_PresentInBuiltTree`

**`DialogueSessionTests.cs`**  
Tests the conversation state machine.

- `Advance_LineNode_SetsDisplayTextAndState`
- `Advance_LineNode_AutoAdvancesToNextNode`
- `Advance_ChoiceNode_ExposesChoices`
- `Advance_ChoiceNode_HidesChoicesFailingCondition`
- `Select_ValidChoice_AppliesEffectsAndAdvances`
- `Select_InvalidIndex_ReturnsFalse`
- `Select_ChoiceFailingCondition_ReturnsFalse`
- `Advance_SequenceNode_ProcessesChildrenInOrder`
- `Advance_PoolNode_PicksEntryAndDisplays`
- `Advance_PoolNode_NoEligibleEntries_EndsConversation`
- `Advance_NodeConditionFalse_SkipsNode`
- `Advance_EmptyQueue_SetsStateToComplete`
- `OnEnterEffect_AppliedOnEnter`

**`MinMaxTests.cs`**

- `Contains_NoLimits_AlwaysReturnsTrue`
- `Contains_MinOnly_BelowMin_ReturnsFalse`
- `Contains_MinOnly_AtMin_ReturnsTrue`
- `Contains_MaxOnly_AboveMax_ReturnsFalse`
- `Contains_MaxOnly_AtMax_ReturnsTrue`
- `Contains_BothLimits_WithinRange_ReturnsTrue`
- `Contains_BothLimits_OutsideRange_ReturnsFalse`

---

### Group 2 — Behavior System

**`ActionQueueTests.cs`**

- `Enqueue_SingleTask_PeekReturnsIt`
- `Enqueue_HigherPriority_MovesToFront`
- `Enqueue_LowerPriority_GoesToBack`
- `Enqueue_SamePriority_OrderIsStable`
- `Dequeue_ReturnsHighestPriority`
- `Dequeue_EmptyQueue_ReturnsNull`
- `Peek_DoesNotRemoveTask`
- `Clear_EmptiesQueue`
- `Count_ReflectsEnqueueAndDequeue`

**`ActorScheduleTests.cs`**

- `GetModeForHour_NoEntries_ReturnsEmpty`
- `GetModeForHour_SingleEntry_MatchesRange`
- `GetModeForHour_SingleEntry_OutsideRange_ReturnsEmpty`
- `GetModeForHour_MultipleEntries_ReturnsFirstMatch`
- `GetModeForHour_EdgeHour_0_Matches`
- `GetModeForHour_EdgeHour_23_Matches`
- `GetModeForHour_OverlappingRanges_FirstWins`

**`ActorComponentPerceptionTests.cs`**

- `CanPerceive_SamePosition_ReturnsTrue`
- `CanPerceive_WithinChebyshevRange_ReturnsTrue`
- `CanPerceive_ExactlyAtRange_ReturnsTrue`
- `CanPerceive_BeyondRange_ReturnsFalse`
- `CanPerceive_DiagonalDistance_UsesChebyshevNotManhattan`

---

### Group 3 — World Primitives

**`BoundsTests.cs`**

- `Constructor_Valid_SetsProperties`
- `Constructor_RightLessThanLeft_Throws`
- `Constructor_BottomLessThanTop_Throws`
- `Width_IsRightMinusLeft`
- `Height_IsBottomMinusTop`
- `Intersects_Overlapping_ReturnsTrue`
- `Intersects_Touching_ReturnsFalse`  *(verify edge behavior)*
- `Intersects_Separate_ReturnsFalse`
- `Intersects_OneContainsOther_ReturnsTrue`
- `Intersects_ZeroSizeBounds_CornerCase`

**`ObjectHandleManagerTests.cs`**

- `Allocate_ReturnsValidHandle`
- `Allocate_Sequential_AllDifferent`
- `AllocateToSlot_SpecificSlot_UsedByHandle`
- `GetSlot_ValidHandle_ReturnsSlot`
- `GetSlot_InvalidHandle_ReturnsNull`
- `Deallocate_ValidHandle_FreesSlot`
- `Deallocate_ThenAllocate_SlotReused`
- `Deallocate_InvalidHandle_NoOp`
- `ActiveHandles_CountsAllocated`

**`MovementStateTests.cs`**

- `IsMoving_EmptyPath_ReturnsFalse`
- `IsMoving_WithPath_ReturnsTrue`
- `ClearMovement_ResetsPath`
- `ClearMovement_ResetsFinalTarget`
- `ClearMovement_ResetsStepAccumulator`

---

### Group 4 — Quest, Inventory, Reputation

**`QuestLogTests.cs`**

- `HasActiveQuest_NotStarted_ReturnsFalse`
- `HasActiveQuest_Active_ReturnsTrue`
- `HasCompletedQuest_NotStarted_ReturnsFalse`
- `HasCompletedQuest_Completed_ReturnsTrue`
- `CanStart_NeverStarted_ReturnsTrue`
- `CanStart_AlreadyActive_ReturnsFalse`
- `CanStart_AlreadyCompleted_ReturnsFalse`
- `StartQuest_AddsToActiveList`
- `StartQuest_AlreadyActive_IsIdempotent`
- `StartQuest_AlreadyCompleted_NoOp`
- `TryComplete_ActiveQuest_MovesToCompleted`
- `TryComplete_InactiveQuest_ReturnsFalse`
- `ActiveQuests_ReflectsCurrentState`

**`InventoryComponentTests.cs`**

- `TryAdd_SingleItem_NonStackable_Succeeds`
- `TryAdd_Stackable_AddsToExistingSlot`
- `TryAdd_Stackable_NewSlotWhenExistingFull`
- `TryAdd_ExceedsMaxStack_ReturnsRemainder`
- `TryAdd_MaxSlotsReached_NonStackable_ReturnsFalse`
- `TryAdd_ItemKindNotAllowed_ReturnsFalse`
- `TryRemove_ExactCount_Succeeds`
- `TryRemove_MoreThanAvailable_ReturnsFalse`
- `TryRemove_AcrossMultipleSlots_Succeeds`
- `TryRemove_EmptiesSlot_SlotRemoved`
- `CountOf_Empty_ReturnsZero`
- `CountOf_MultipleSlots_ReturnsSum`

**`ReputationLogTests.cs`**

- `GetReputation_UnknownFaction_ReturnsZero`
- `GetReputation_KnownFaction_ReturnsScore`
- `AddReputation_Positive_Increments`
- `AddReputation_Negative_Decrements`
- `AddReputation_ClampsAtMinimum`
- `AddReputation_ClampsAtMaximum`
- `GetTier_Exalted_CorrectThreshold`
- `GetTier_Hostile_CorrectThreshold`
- `GetTier_Neutral_AtZero`
- `GetTier_AllBoundaries_Correct`
- `CaseInsensitiveFactionId`

---

### Group 5 — AI Systems

**`AlertBoardTests.cs`**

- `BeginTick_MakesPostedAlertsVisible`
- `Post_BeforeBeginTick_NotVisible`
- `GetNearbyAlerts_NoAlerts_ReturnsEmpty`
- `GetNearbyAlerts_WithinRadius_Returned`
- `GetNearbyAlerts_BeyondRadius_NotReturned`
- `GetNearbyAlerts_ExactRadius_Included`
- `GetNearbyAlerts_UsesChebyshevDistance`
- `GetNearbyAlerts_FiltersByKind`
- `DoubleBuffer_PreviousTickAlerts_ClearedNextTick`

**`LineOfSightTests.cs`**

- `Check_SamePoint_ReturnsTrue`
- `Check_ClearLine_ReturnsTrue`
- `Check_BlockedByObstacle_ReturnsFalse`
- `Check_AdjacentObstacle_BlocksLine`
- `Check_NoObstacles_ReturnsTrue`
- `Check_DiagonalLine_ClearPath_ReturnsTrue`

---

### Group 6 — Data Registry

**`StaticDataTests.cs`**

- `RegisterItem_RetrievableById`
- `GetItem_UnknownId_ReturnsNull`
- `GetItem_CaseInsensitive`
- `RegisterQuest_RetrievableById`
- `GetQuest_UnknownId_ReturnsNull`
- `RegisterFaction_RetrievableById`
- `GetFaction_UnknownId_ReturnsNull`
- `DuplicateId_Throws`  *(verify registration contract)*

---

### Group 7 — Pathfinding Additions

Additions to the existing `AStarPathfinderTests.cs`:

- `FindPath_LargeMovers_AccountsForFullFootprint`
- `FindPath_DiagonalPath_ShorterThanCardinal`

New file **`PathGridTests.cs`**:

- `IsPassable_EmptyGrid_ReturnsTrue`
- `IsPassable_MoverAtObstacle_ReturnsFalse`
- `IsPassable_MoverPartiallyOverlaps_ReturnsFalse`
- `IsPassable_MoverClearOfAllObstacles_ReturnsTrue`
- `IsPassable_LargeMover_FullFootprintChecked`

---

## Editor Tests (NPCChat.Tests or separate NPCChat.Editor.Tests)

Lower priority — the Editor is an internal tool that will not ship with the Unity build. Add these after Core coverage is solid.

**`UserSettingsRepositoryTests.cs`**

Tests window state persistence. Requires temp directory setup/teardown.

- `Save_CreatesFile`
- `Restore_LoadsFromFile`
- `Restore_MissingFile_NoOp`
- `Restore_InvalidJson_DoesNotThrow`
- `RoundTrip_SaveAndRestore_PreservesValues`
- `IsUsableWindowBounds_TooSmall_ReturnsFalse`
- `IsUsableWindowBounds_ValidSize_ReturnsTrue`
- `Restore_BoundsOffScreen_UsesDefaults`

---

## Notes

- **Unity compatibility**: Do not use MSTest-specific attributes ([DataTestMethod], [DataRow]) in tests that will eventually move to Unity's NUnit-based runner. Use plain [TestMethod] + separate test methods, or helper loops, until a framework decision is made.
- **Seeded Random**: Always pass an explicit `new Random(seed)` to `DialoguePicker` tests to make them deterministic. Document the seed in a comment.
- **No mocking framework**: Avoid adding Moq or NSubstitute unless there's a clear need — most Core logic accepts plain delegates or readonly structs, which can be supplied inline.
- **Test helpers**: Add a static `TestHelpers.cs` in NPCChat.Tests for shared tree-building and actor-creation utilities as the suite grows.
