#!/bin/zsh

dotnet-ef migrations add --project MusicStreamingService.Data --startup-project MusicStreamingService  "$1"
dotnet ef dbcontext optimize --project MusicStreamingService.Data --startup-project MusicStreamingService
