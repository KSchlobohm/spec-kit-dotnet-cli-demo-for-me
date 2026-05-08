namespace TimezoneMeetingCli.Models;

public record MeetingProposal(
    DateTimeOffset BaseTime,
    List<LocalizedTimeResult> ParticipantResults
);